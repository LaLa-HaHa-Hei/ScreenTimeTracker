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
        private const string LogDirName = "TrackererLogs";
        private static readonly string SettingsFileName = "TrackerSettings.json";
        private static int _intervalMs = 1000;
        private static int _saveThreshold = 10;
        private static readonly string DataDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DataDirPath);
        private static readonly string IconDirPath = Path.Combine(DataDirPath, Shared.Constants.FilePaths.IconDirName);
        private static readonly string LogDirPath = Path.Combine(DataDirPath, LogDirName);

        static async Task<int> Main(string[] args)
        {
            if (!EnsureSingleInstance())
                return 1;

            {// 创建需要的文件夹
                if (!Directory.Exists(DataDirPath))
                    Directory.CreateDirectory(DataDirPath);
                if (!Directory.Exists(IconDirPath))
                    Directory.CreateDirectory(IconDirPath);
                if (!Directory.Exists(LogDirPath))
                    Directory.CreateDirectory(LogDirPath);
            }

            if (File.Exists(SettingsFileName))
            {
                string jsonString = File.ReadAllText(SettingsFileName);
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
                        .WriteTo.File(Path.Combine(LogDirPath, "log-.txt"),
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7)
                        .Enrich.FromLogContext()
                        .MinimumLevel.Warning()
                        .MinimumLevel.Override("Tracker", LogEventLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    var dbPath = Path.Combine(DataDirPath, Shared.Constants.FilePaths.DbFileName);
                    services.AddDbContext<ScreenTimeContext>(options =>
                        options.UseSqlite($"Data Source={dbPath}"));
                    services.AddSingleton(new AppOptions()
                    {
                        IntervalMs = _intervalMs,
                        SaveThreshold = _saveThreshold,
                        IconDirPath = IconDirPath
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
            File.WriteAllText(SettingsFileName, defaultSettings);
        }
    }
}