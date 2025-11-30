using Mediator;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;

namespace ScreenTimeTracker.Desktop.BackgroundServices;

class AggregationSettingsChangedHandler(AggregationWorker worker)
    : INotificationHandler<AggregationSettingsChangedNotification>
{
    private readonly AggregationWorker _worker = worker;

    public ValueTask Handle(AggregationSettingsChangedNotification notification, CancellationToken cancellationToken)
    {
        _worker.OnSettingsChanged();
        return ValueTask.CompletedTask;
    }
}