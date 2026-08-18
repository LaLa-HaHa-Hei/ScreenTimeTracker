using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public class UserBecameActiveHandler(
    IForegroundWindowMonitor foregroundWindowMonitor,
    ForegroundWindowProcessor foregroundWindowProcessor
) : INotificationHandler<UserBecameActiveEvent>
{
    public async ValueTask Handle(
        UserBecameActiveEvent notification,
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
