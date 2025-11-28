using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Common.Services;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetAggregationSettings;

namespace ScreenTimeTracker.Desktop.BackgroundServices;

internal class AggregationWorker(ILogger<AggregationWorker> logger, AggregationService aggregationService, IServiceProvider serviceProvider) : BackgroundService, INotificationHandler<AggregationSettingsChangedNotification>
{
    private readonly ILogger<AggregationWorker> _logger = logger;
    private readonly AggregationService _aggregationService = aggregationService;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private PeriodicTimer? _timer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AggregationWorker is starting.");

        using (var scope = _serviceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            AggregationSettingsDto settings =
                await mediator.Send(new GetAggregationSettingsQuery(), stoppingToken);

            _timer = new PeriodicTimer(settings.PollingInterval);
        }

        do
        {
#if DEBUG
            await _aggregationService.SummarizeHourlyDataAsync();
#else
            try
            {
                await _aggregationService.SummarizeHourlyDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summarizing hourly data.");
            }
#endif 
        } while (await _timer.WaitForNextTickAsync(stoppingToken));

        _logger.LogInformation("AggregationWorker is stopping.");
    }

    public ValueTask Handle(AggregationSettingsChangedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Configuration changed.");

        if (_timer is not null)
        {
            _logger.LogInformation("Recreating aggregation timer");
            // 重新创建 timer
            _timer.Dispose();
            _timer = new PeriodicTimer(notification.Settings.PollingInterval);
        }

        return ValueTask.CompletedTask;
    }
}