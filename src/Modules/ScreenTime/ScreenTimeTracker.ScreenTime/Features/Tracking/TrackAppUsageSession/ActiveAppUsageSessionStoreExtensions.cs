using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsageSession;

public static class ActiveAppUsageSessionStoreExtensions
{
    public static async Task StageSaveToAsync(
        this IActiveAppUsageSessionStore activeSessionStore,
        ScreenTimeDbContext context,
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        if (activeSessionStore.Current is null)
            return;

        var existing = await context.AppUsageSessions.FirstOrDefaultAsync(
            x =>
                x.AppId == activeSessionStore.Current.AppId
                && x.StartTime == activeSessionStore.Current.StartTime,
            cancellationToken
        );

        if (existing is null)
        {
            var sessionEntity = AppUsageSession.Create(
                appId: activeSessionStore.Current.AppId,
                startTime: activeSessionStore.Current.StartTime,
                endTime: now
            );

            context.AppUsageSessions.Add(sessionEntity);
        }
        else
            existing.UpdateEndTime(now);
    }
}
