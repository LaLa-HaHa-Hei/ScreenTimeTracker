using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Tracker.Options;
using Tracker.Services;
using Tracker.Services.Interfaces;
using System.Text.Json;

namespace Tracker
{
    class Program
    {
        private static Mutex? _mutex;
        private static readonly string _logDirPath = Path.Combine(Shared.Constants.FilePaths.DataDirPath, "TrackererLogs");
        private static readonly string _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "TrackerSettings.json");
        private static int _intervalMs = 1000;
        private static int _saveThreshold = 10;
        private static readonly string _absluteDataDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DataDirPath);
        private static readonly string _absluteLogDirPath = Path.Combine(AppContext.BaseDirectory, _logDirPath);

        static async Task<int> Main(string[] args)
        {
            if (!EnsureSingleInstance())
                return 1;

            {// 创建需要的文件夹
                if (!Directory.Exists(_absluteDataDirPath))
                    Directory.CreateDirectory(_absluteDataDirPath);
                string absluteIconDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.IconDirPath);
                if (!Directory.Exists(absluteIconDirPath))
                    Directory.CreateDirectory(absluteIconDirPath);
                if (!Directory.Exists(_absluteLogDirPath))
                    Directory.CreateDirectory(_absluteLogDirPath);
            }

            if (File.Exists(_settingsFilePath))
            {
                string jsonString = File.ReadAllText(_settingsFilePath);
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("IntervalMs", out var intervalProp) &&
                    root.TryGetProperty("SaveThreshold", out var thresholdProp))
                {
                    var intervalMs = intervalProp.GetInt32();
                    var saveThreshold = thresholdProp.GetInt32();
                    if (intervalMs > 0 && saveThreshold > 0)
                    {
                        _intervalMs = intervalMs;
                        _saveThreshold = saveThreshold;
                    }
                    else
                        WriteDefaultSettings();
                }
                else
                    WriteDefaultSettings();
            }
            else
                WriteDefaultSettings();

            var host = Host.CreateDefaultBuilder()
                .UseSerilog((context, services, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .WriteTo.File(Path.Combine(_absluteLogDirPath, "log-.txt"),
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7)
                        .Enrich.FromLogContext()
                        .MinimumLevel.Warning()
                        .MinimumLevel.Override("Tracker", LogEventLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    var dbPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DbFilePath);
                    services.AddDbContext<ScreenTimeContext>(options =>
                        options.UseSqlite($"Data Source={dbPath}"));
                    services.AddSingleton(new AppOptions()
                    {
                        IntervalMs = _intervalMs,
                        SaveThreshold = _saveThreshold,
                        IconDirPath = Shared.Constants.FilePaths.IconDirPath
                    });
                    services.AddSingleton<IForegroundWindowService, ForegroundWindowService>();
                    services.AddSingleton<IIconService, IconService>();
                    services.AddSingleton<IProcessInfoService, ProcessInfoService>();
                    services.AddSingleton<IUsageAggregator, UsageAggregator>();
                    services.AddHostedService<UsageTracker>();
                })
                .Build();

            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ScreenTimeContext>();
                context.Database.Migrate();
            }

            await host.RunAsync();

            _mutex?.ReleaseMutex(); // 释放互斥锁
            return 0;
        }

        private static bool EnsureSingleInstance()
        {
            _mutex = new Mutex(true, "ScreenTimeTrackerUniqueMutexName", out bool createdNew);
            if (!createdNew)
            {
                return false;
            }
            return true;
        }

        private static void WriteDefaultSettings()
        {
            string defaultSettings = @"{
    ""IntervalMs"": 1000,
    ""SaveThreshold"": 10
}";
            File.WriteAllText(_settingsFilePath, defaultSettings);
        }
    }
}