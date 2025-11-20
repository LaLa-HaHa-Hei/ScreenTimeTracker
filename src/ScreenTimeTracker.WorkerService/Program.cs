using ScreenTimeTracker.Application;
using ScreenTimeTracker.Infrastructure;
using ScreenTimeTracker.Infrastructure.Interfaces;
using ScreenTimeTracker.WorkerService;
using Serilog;

namespace ScreenTimeTracker.WorkerService
{
    public class Program()
    {
        private static Mutex? _mutex;

        public static async Task Main(string[] args)
        {
            _mutex = new(true, "ScreenTimeTrackerWorkerServiceUniqueMutexName", out bool createdNew);
            if (!createdNew)
                return;

            // 切换工作目录为程序所在目录
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration
                .AddJsonFile("workerservice.appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"workerservice.appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

            // 添加日志并配置
            builder.Services.AddSerilog((services, loggerConfig) =>
            {
                loggerConfig.ReadFrom.Configuration(builder.Configuration);
            });

            // 注入其他层的服务
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.AddHostedService<TrackerWorker>();
            builder.Services.AddHostedService<AggregationWorker>();

            var host = builder.Build();
            await InitializePersistenceAsync(host);

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting Worker Service host.");

            host.Run();
        }

        static async Task InitializePersistenceAsync(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var initializer = services.GetRequiredService<IDbContextInitializer>();
            await initializer.InitializeAsync();
        }
    }
}
