using Mediator;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public class SystemSuspendingHandler(
    ActiveAppUsageSessionStore activeSessionStore,
    ScreenTimeDbContext context,
    TimeProvider timeProvider
) : INotificationHandler<SystemSuspendingEvent>
{
    public async ValueTask Handle(
        SystemSuspendingEvent notification,
        CancellationToken cancellationToken
    )
    {
        var activeSession = activeSessionStore.Take();
        if (activeSession is null)
            return;

        var now = timeProvider.GetUtcNow();
        await context.PersistActiveSessionAsync(activeSession, now, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return;
    }
}
