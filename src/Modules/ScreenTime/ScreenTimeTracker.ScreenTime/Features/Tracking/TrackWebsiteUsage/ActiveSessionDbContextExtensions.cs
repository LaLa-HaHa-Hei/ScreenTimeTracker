using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;

public static class ActiveSessionDbContextExtensions
{
    public static async Task PersistActiveSessionAsync(
        this ScreenTimeDbContext context,
        ActiveWebsiteUsageSessionState activeSessionState,
        DateTimeOffset endTime,
        CancellationToken cancellationToken
    )
    {
        if (activeSessionState.LastActiveAt <= activeSessionState.StartTime)
            return;

        var existing = await context.WebsiteUsageSessions.FirstOrDefaultAsync(
            x =>
                x.WebsiteId == activeSessionState.WebsiteId
                && x.StartTime == activeSessionState.StartTime,
            cancellationToken
        );

        if (existing is null)
        {
            var sessionEntity = WebsiteUsageSession.Create(
                websiteId: activeSessionState.WebsiteId,
                new TimeRange(activeSessionState.StartTime, endTime)
            );

            context.WebsiteUsageSessions.Add(sessionEntity);
        }
        else
            existing.UpdateEndTime(endTime);
    }
}
