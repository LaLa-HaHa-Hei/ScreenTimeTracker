using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public static class ActiveAppUsageSessionStoreExtensions
{
    public static async Task PersistSessionAsync(
        this ActiveAppUsageSessionState activeSessionState,
        ScreenTimeDbContext context,
        DateTimeOffset endTime,
        CancellationToken cancellationToken
    )
    {
        if (endTime <= activeSessionState.StartTime)
            return;

        var existing = await context.AppUsageSessions.FirstOrDefaultAsync(
            x => x.AppId == activeSessionState.AppId && x.StartTime == activeSessionState.StartTime,
            cancellationToken
        );

        if (existing is null)
        {
            var sessionEntity = AppUsageSession.Create(
                appId: activeSessionState.AppId,
                startTime: activeSessionState.StartTime,
                endTime: endTime
            );

            context.AppUsageSessions.Add(sessionEntity);
        }
        else
            existing.UpdateEndTime(endTime);
    }
}
