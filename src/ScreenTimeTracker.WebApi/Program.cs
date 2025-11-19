using ScreenTimeTracker.Application;
using ScreenTimeTracker.Infrastructure;
using ScreenTimeTracker.Infrastructure.Interfaces;
using Serilog;


Mutex _mutex = new(true, "ScreenTimeTrackerWebApiUniqueMutexName", out bool createdNew);
if (!createdNew)
    return;

// 切换工作目录为程序所在目录
Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("webapi.appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"webapi.appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

// 添加日志并配置
builder.Services.AddSerilog((services, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(builder.Configuration);
});

// 注入其他层的服务
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddControllers();
// 文档url：/openapi/v1.json
builder.Services.AddOpenApi();

var app = builder.Build();

// 初始化持久化层
await InitializePersistenceAsync(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 启用跨域
app.UseCors(builder => builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseAuthorization();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting WebApi host.");

app.Lifetime.ApplicationStarted.Register(() =>
{
    logger.LogInformation("Listening on: {Addresses}", string.Join(", ", app.Urls));
});

app.Run();

static async Task InitializePersistenceAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;

    var initializer = services.GetRequiredService<IDbContextInitializer>();
    await initializer.InitializeAsync();
}
