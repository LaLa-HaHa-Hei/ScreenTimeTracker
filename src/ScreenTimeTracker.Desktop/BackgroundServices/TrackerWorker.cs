using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Common.Services;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetTrackerSettings;

namespace ScreenTimeTracker.Desktop.BackgroundServices;

class TrackerWorker(ILogger<TrackerWorker> logger, TrackerService trackerService, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ILogger<TrackerWorker> _logger = logger;
    private readonly TrackerService _trackerService = trackerService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private CancellationTokenSource? _configChangedCts;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrackerWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            TrackerSettingsDto settings;
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                settings = await mediator.Send(new GetTrackerSettingsQuery(), stoppingToken);
            }

            _configChangedCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _configChangedCts.Token);
            using var timer = new PeriodicTimer(settings.PollingInterval);

            try
            {
                while (await timer.WaitForNextTickAsync(linkedCts.Token))
                {
                    try
                    {
                        await _trackerService.RecordActivityIntervalAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error recording activity interval.");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 忽略取消异常，继续判断流程
                // 两种情况会抛出此异常：
                // 1. stoppingToken 取消了 -> 服务正在停止 -> 外层 while 会结束
                // 2. _configChangedCts 取消了 -> 配置变了 -> 捕获异常，进入外层 while 的下一次迭代（重建 Timer）
            }
            finally
            {
                _configChangedCts?.Dispose();
                _configChangedCts = null;
            }

            // 如果是整个应用停止，则跳出外层循环
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            _logger.LogInformation("Tracker settings changed detected. Restarting timer loop...");
        }

        _logger.LogInformation("TrackerWorker is stopping.");
    }

    public void OnSettingsChanged()
    {
        _configChangedCts?.Cancel();
    }
}
