using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer;
using ScreenTimeTracker.Desktop.BackgroundServices;
using ScreenTimeTracker.Desktop.Services;
using ScreenTimeTracker.Desktop.UI;
using ScreenTimeTracker.Infrastructure;
using Serilog;

namespace ScreenTimeTracker.Desktop;

public class Program
{
    private static Mutex? _mutex;

    [STAThread]
    public static void Main(string[] args)
    {
        _mutex = new(true, "ScreenTimeTrackerDesketopUniqueMutexName", out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show("程序已经在运行，请查看托盘处！", "注意！");
            return;
        }


        // 切换工作目录为程序所在目录
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var builder = WebApplication.CreateBuilder(args);

        // 添加日志并配置
        builder.Services.AddSerilog((services, loggerConfig) =>
        {
            loggerConfig.ReadFrom.Configuration(builder.Configuration);
        });

        // 注入其他层的服务
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationLayerServices();

        // UI
        builder.Services.AddSingleton<NotifyIconViewModel>();
        // BackgroundServices
        builder.Services.AddHostedService<TrackerWorker>();
        builder.Services.AddHostedService<AggregationWorker>();
        // WebApi 
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi(); // 文档url：/openapi/v1.json
        // CQRS
        builder.Services.AddMediator(options =>
        {
            options.Namespace = "ScreenTimeTracker.Mediator";
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
        // 服务
        builder.Services.AddSingleton<AppRuntimeData>();

        var host = builder.Build();

        // Configure the HTTP request pipeline.
        if (host.Environment.IsDevelopment())
        {
            host.MapOpenApi();
        }

        host.UseStaticFiles(); // 托管前端静态文件
        // 启用跨域
        host.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
        host.UseAuthorization();
        host.MapControllers();
        host.MapFallbackToFile("index.html");

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        host.Start();

        var runtimeData = host.Services.GetRequiredService<AppRuntimeData>();
        var address = host.Urls.FirstOrDefault();
        if (address != null)
        {
            runtimeData.BaseUrl = address;
            logger.LogInformation("Listening on: {Url}", address);
        }

        var app = new App
        {
            ServiceProvider = host.Services
        };
        app.InitializeComponent();
        app.Run();

        logger.LogInformation("UI stoped");
        host.StopAsync().GetAwaiter().GetResult();
    }
}
