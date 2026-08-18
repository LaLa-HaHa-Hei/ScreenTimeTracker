using System.Globalization;
using System.Text.Json.Serialization;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.Desktop;
using ScreenTimeTracker.Desktop.Hosting;
using ScreenTimeTracker.Desktop.Platforms;
using ScreenTimeTracker.Desktop.UI.Services;
using ScreenTimeTracker.Desktop.UI.State;
using ScreenTimeTracker.DesktopSettings;
using ScreenTimeTracker.ScreenTime;
using Serilog;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

[assembly: RootNamespace("ScreenTimeTracker.Desktop")]

// 切换工作目录为程序所在目录，规定所有相对路径都是相对于程序所在目录
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

// 临时日志配置
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .WriteTo.File(
        "./startup.log",
        shared: true,
        fileSizeLimitBytes: 1 * 1024 * 1024,
        rollOnFileSizeLimit: true,
        retainedFileCountLimit: 2,
        formatProvider: CultureInfo.InvariantCulture
    )
    .CreateBootstrapLogger();

Log.Information("Application Started.");

// 全局异常兜底
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    Log.Fatal(e.ExceptionObject as Exception, "AppDomain Unhandled Exception occurred.");
    Log.CloseAndFlush();
};
TaskScheduler.UnobservedTaskException += (sender, e) =>
{
    Log.Fatal(e.Exception, "Unobserved task exception occurred.");
};

try
{
    var builder = WebApplication.CreateBuilder();
    // 通用服务
    builder.Services.AddSerilog(
        (services, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(builder.Configuration);
        }
    );
    builder.Services.AddMediator(options =>
    {
        options.Namespace = "ScreenTimeTracker.Mediator";
        options.ServiceLifetime = ServiceLifetime.Scoped;
    });
    builder.Services.AddSingleton(TimeProvider.System);
    // Web API
    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddSignalR();
    builder.Services.AddFastEndpoints();
    builder.Services.SwaggerDocument();
    builder.Services.AddCors();
    builder.Services.AddSingleton<IServerUrlProvider, ServerUrlProvider>();
    // 桌面
    builder.Services.AddLocalization(options =>
    {
        options.ResourcesPath = "Resources";
    });
    builder.Services.AddSingleton<DesktopLocalSettingsProvider>();
    builder.Services.AddSingleton<IDesktopLocalSettingsProvider>(sp =>
        sp.GetRequiredService<DesktopLocalSettingsProvider>()
    );
    builder.Services.AddHostedService<DesktopLocalSettingsProviderInitializer>();
    builder.Services.AddSingleton<IAppUIManager, AppUIManager>();
    builder.Services.AddSingleton<IWindowPlacementStore, WindowPlacementStore>();
    // 平台特异
    if (OperatingSystem.IsWindows())
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(5, 1, 2600))
        {
            builder.Services.AddSingleton<ISingleInstanceLock, WindowsSingleInstanceLock>();
            builder.Services.AddSingleton<IInstanceMessenger, WindowsInstanceMessenger>();
            builder.Services.AddSingleton<ITrayService, WindowsTrayService>();
        }
        else
            throw new PlatformNotSupportedException("Only Windows5.1.2600 or later is supported.");
    }
    else if (OperatingSystem.IsLinux())
    {
        builder.Services.AddSingleton<ISingleInstanceLock, LinuxSingleInstanceLock>();
        builder.Services.AddSingleton<IInstanceMessenger, LinuxInstanceMessenger>();
        builder.Services.AddSingleton<ITrayService, LinuxTrayService>();
    }
    else
    {
        throw new PlatformNotSupportedException("Only Windows or Linux is supported.");
    }

    // 模块注册
    builder.Services.AddScreenTimeServices(builder.Configuration);
    builder.Services.AddDesktopSettingsServices(builder.Configuration);

    using WebApplication app = builder.Build();

    // 单例检测
    var singleInstanceLock = app.Services.GetRequiredService<ISingleInstanceLock>();
    var instanceMessenger = app.Services.GetRequiredService<IInstanceMessenger>();
    if (!singleInstanceLock.TryAcquire())
    {
        Log.Information("Application is already running.");
        if (!await instanceMessenger.SendMessageAsync("OpenUI"))
        {
            Log.Error("Failed to send message to existing instance.");
            ShowError("Program is already running, please check the tray icon.");
        }
        Log.CloseAndFlush();
        return;
    }
    instanceMessenger.MessageReceived += (sender, e) =>
    {
        if (e.Message == "OpenUI")
        {
            Log.Information("Received message from existing instance to show UI.");
            var appUIManager = app.Services.GetRequiredService<IAppUIManager>();
            appUIManager.OpenUI();
        }
    };
    await instanceMessenger.StartListeningAsync();

    // 中间件
    app.UseExceptionHandler();
    app.UseStaticFiles();
    app.UseCors(cors =>
    {
        cors.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
    app.UseFastEndpoints(config =>
    {
        config.Endpoints.RoutePrefix = "api";
        // 枚举 <-> 字符串，保持枚举值名称不转换，禁用数字枚举
        config.Serializer.Options.Converters.Add(
            new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false)
        );

        // OptionalValue<T> JSON 转换器
        config.Serializer.Options.Converters.Add(new OptionalValueJsonConverterFactory());
    });
    app.UseSwaggerGen(); // 文档url: /swagger/v1/swagger.json
    app.MapFallbackToFile("index.html"); // SPA回退

    app.Lifetime.ApplicationStopped.Register(() =>
    {
        Log.Information("Application Stopped.");
    });

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var serverUrlProvider = app.Services.GetRequiredService<IServerUrlProvider>();
        var serverUrl = serverUrlProvider.GetServerUrl();
        Log.Information("Kestrel server address: {address}", serverUrl);

        // 初始化托盘图标
        var trayService = app.Services.GetRequiredService<ITrayService>();
        trayService.Show();
        // 非静默启动时打开UI
        var desktopLocalSettingsProvider =
            app.Services.GetRequiredService<IDesktopLocalSettingsProvider>();
        if (!desktopLocalSettingsProvider.IsSilentStartEnabled)
        {
            var appUIManager = app.Services.GetRequiredService<IAppUIManager>();
            appUIManager.OpenUI();
        }
    });

    try
    {
        app.Run();
    }
    catch (IOException ex) when (ex.InnerException is AddressInUseException)
    {
        Log.Error(ex, "Address already in use.");
        ShowError(
            "Address already in use, please close other instances or change the port in appsettings.json."
        );
        await app.StopAsync();
    }
    Log.CloseAndFlush();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
    ShowError("Application terminated unexpectedly, please check the log.");
    Log.CloseAndFlush();
    Environment.Exit(1);
}

static void ShowError(string message)
{
    if (OperatingSystem.IsWindowsVersionAtLeast(5, 0, 0))
    {
        PInvoke.MessageBox(
            default,
            message,
            "Error",
            MESSAGEBOX_STYLE.MB_ICONERROR | MESSAGEBOX_STYLE.MB_OK
        );
    }
    else
        throw new PlatformNotSupportedException("Only Windows5.0 or later is supported.");
}
