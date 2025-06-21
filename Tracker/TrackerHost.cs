using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.Text.Json;
using Tracker.Options;
using Tracker.Services;
using Tracker.Services.Interfaces;

namespace Tracker
{
    public class TrackerHost
    {
        private IHost? _host;
        private readonly string _logDirPath = Path.Combine(Shared.Constants.FilePaths.DataDirPath, "TrackererLogs");
        private readonly string _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "TrackerSettings.json");
        private readonly string _absluteDataDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.DataDirPath);
        private readonly string _absluteLogDirPath;
        private int _intervalMs = 1000;
        private int _saveThreshold = 10;

        public TrackerHost()
        {
            _absluteLogDirPath = Path.Combine(AppContext.BaseDirectory, _logDirPath);
        }

        public async Task StartAsync()
        {
            PrepareFolders();
            LoadOrWriteDefaultSettings();

            _host = Host.CreateDefaultBuilder()
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

                    services.AddSingleton(new AppOptions
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

            // 自动应用数据库迁移
            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ScreenTimeContext>();
                context.Database.Migrate();
            }

            await _host.StartAsync();
        }

        public async Task StopAsync()
        {
            if (_host != null)
            {
                await _host.StopAsync();
                await _host.WaitForShutdownAsync();
            }
        }

        private void PrepareFolders()
        {
            if (!Directory.Exists(_absluteDataDirPath))
                Directory.CreateDirectory(_absluteDataDirPath);

            var absluteIconDirPath = Path.Combine(AppContext.BaseDirectory, Shared.Constants.FilePaths.IconDirPath);
            if (!Directory.Exists(absluteIconDirPath))
                Directory.CreateDirectory(absluteIconDirPath);

            if (!Directory.Exists(_absluteLogDirPath))
                Directory.CreateDirectory(_absluteLogDirPath);
        }

        private void LoadOrWriteDefaultSettings()
        {
            if (File.Exists(_settingsFilePath))
            {
                string jsonString = File.ReadAllText(_settingsFilePath);
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                if (root.TryGetProperty("IntervalMs", out var intervalProp) &&
                    root.TryGetProperty("SaveThreshold", out var thresholdProp))
                {
                    var intervalMs = intervalProp.GetInt32();
                    var saveThreshold = thresholdProp.GetInt32();
                    if (intervalMs > 0 && saveThreshold > 0)
                    {
                        _intervalMs = intervalMs;
                        _saveThreshold = saveThreshold;
                        return;
                    }
                }
            }

            WriteDefaultSettings();
        }

        private void WriteDefaultSettings()
        {
            var defaultSettings = new
            {
                IntervalMs = 1000,
                SaveThreshold = 10
            };
            string json = JsonSerializer.Serialize(defaultSettings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFilePath, json);
        }
    }
}
