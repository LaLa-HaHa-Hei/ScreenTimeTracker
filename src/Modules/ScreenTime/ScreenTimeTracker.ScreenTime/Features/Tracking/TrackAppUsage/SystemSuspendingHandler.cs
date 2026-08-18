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
        if (activeSessionStore.Current is null)
            return;

        var now = timeProvider.GetUtcNow();
        await context.PersistActiveSessionAsync(activeSessionStore.Current, now, cancellationToken);
        activeSessionStore.Current = null;
        await context.SaveChangesAsync(cancellationToken);

        return;
    }
}
