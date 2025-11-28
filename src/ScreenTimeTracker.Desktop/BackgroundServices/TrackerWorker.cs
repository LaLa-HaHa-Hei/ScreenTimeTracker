using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Common.Services;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetTrackerSettings;

namespace ScreenTimeTracker.Desktop.BackgroundServices;

public class TrackerWorker(ILogger<TrackerWorker> logger, TrackerService trackerService, IServiceProvider serviceProvider) : BackgroundService, INotificationHandler<TrackerSettingsChangedNotification>
{
    private readonly ILogger<TrackerWorker> _logger = logger;
    private readonly TrackerService _trackerService = trackerService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private PeriodicTimer? _timer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrackerWorker is starting.");

        using (var scope = _serviceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            TrackerSettingsDto settings =
                await mediator.Send(new GetTrackerSettingsQuery(), stoppingToken);

            _timer = new PeriodicTimer(settings.PollingInterval);
        }

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
#if DEBUG
            await _trackerService.RecordActivityIntervalAsync();
#else
            try
            {
                await _trackerService.RecordActivityIntervalAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording activity interval.");
            }
#endif 
        }

        _logger.LogInformation("TrackerWorker is stopping.");
    }

    public ValueTask Handle(TrackerSettingsChangedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuration changed.");

        if (_timer is not null)
        {
            _logger.LogInformation("Recreating tracker timer");
            // 重新创建 timer
            _timer.Dispose();
            _timer = new PeriodicTimer(notification.Settings.PollingInterval);
        }

        return ValueTask.CompletedTask;
    }
}
