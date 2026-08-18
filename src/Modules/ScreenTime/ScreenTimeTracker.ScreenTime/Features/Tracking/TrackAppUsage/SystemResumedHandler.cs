using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public class SystemResumedHandler(
    IForegroundWindowMonitor foregroundWindowMonitor,
    ForegroundWindowProcessor foregroundWindowProcessor
) : INotificationHandler<SystemResumedEvent>
{
    public async ValueTask Handle(
        SystemResumedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
        foregroundWindowProcessor.Enqueue(
            new ForegroundWindowChangedMessage(windowInfo, DateTimeOffset.Now)
        );

        return;
    }
}
