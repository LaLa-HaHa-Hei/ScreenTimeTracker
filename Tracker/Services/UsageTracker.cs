using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tracker.Options;
using Tracker.Services.Interfaces;

namespace Tracker.Services
{
    internal class UsageTracker(AppOptions trackerOptions,
        IForegroundWindowService foregroundWindowService,
        IProcessInfoService processInfoService,
        IUsageAggregator usageAggregator,
        ILogger<UsageTracker> logger) : BackgroundService
    {
        private readonly AppOptions _trackerOptions = trackerOptions;
        private readonly IForegroundWindowService _foregroundWindowService = foregroundWindowService;
        private readonly IProcessInfoService _processInfoService = processInfoService;
        private readonly IUsageAggregator _usageAggregator = usageAggregator;
        private readonly ILogger<UsageTracker> _logger = logger;

        private int _getWindowCount = 0;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting window recording.");
                await StartTrackingingAsync(stoppingToken);
            }
        }

        public async Task StartTrackingingAsync(CancellationToken cancellationToken = default)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_trackerOptions.IntervalMs));
            int consecutiveFailures = 0;
            const int maxFailures = 5;

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                try
                {
                    Process? process = _foregroundWindowService.GetForegroundWindowProcess();
                    if (process != null)
                    {
                        // 检查进程信息是否存在
                        await _processInfoService.EnsureProcessInfoAsync(process);
                        // 添加到累积数据中
                        _usageAggregator.AddUsage(process.ProcessName, _trackerOptions.IntervalMs);
                    }

                    _getWindowCount++;
                    if (_getWindowCount >= _trackerOptions.SaveThreshold)
                    {
                        await _usageAggregator.SaveAsync();
                        _getWindowCount = 0;
                    }

                    // 成功一轮，重置错误计数
                    consecutiveFailures = 0;
                }
                catch (Exception ex)
                {
                    consecutiveFailures++;
                    _logger.LogError(ex, "Error during recording iteration #{FailureCount}", consecutiveFailures);

                    if (consecutiveFailures >= maxFailures)
                    {
                        _logger.LogCritical("Too many consecutive failures. Stopping the recorder.");
                        break; // 或者 throw; 让 BackgroundService 退出
                    }
                }
            }
        }
    }
}
