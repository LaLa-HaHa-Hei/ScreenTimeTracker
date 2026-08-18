using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public static class ActiveSessionDbContextExtensions
{
    public static async Task PersistActiveSessionAsync(
        this ScreenTimeDbContext context,
        ActiveAppUsageSessionState activeSessionState,
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
                activeSessionState.AppId,
                new TimeRange(activeSessionState.StartTime, endTime)
            );

            context.AppUsageSessions.Add(sessionEntity);
        }
        else
            existing.UpdateEndTime(endTime);
    }
}
