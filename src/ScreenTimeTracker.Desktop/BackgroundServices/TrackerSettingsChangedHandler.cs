using Mediator;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;

namespace ScreenTimeTracker.Desktop.BackgroundServices;

class TrackerSettingsChangedHandler(TrackerWorker worker)
    : INotificationHandler<TrackerSettingsChangedNotification>
{
    private readonly TrackerWorker _worker = worker;

    public ValueTask Handle(TrackerSettingsChangedNotification notification, CancellationToken cancellationToken)
    {
        _worker.OnSettingsChanged();
        return ValueTask.CompletedTask;
    }
}