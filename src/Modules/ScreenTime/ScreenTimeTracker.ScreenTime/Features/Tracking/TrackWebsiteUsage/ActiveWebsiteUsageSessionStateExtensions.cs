using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;

public static class ActiveWebsiteUsageSessionStoreExtensions
{
    public static async Task PersistSessionAsync(
        this ActiveWebsiteUsageSessionState activeSessionState,
        ScreenTimeDbContext context,
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
                startTime: activeSessionState.StartTime,
                endTime: activeSessionState.LastActiveAt
            );

            context.WebsiteUsageSessions.Add(sessionEntity);
        }
        else
            existing.UpdateEndTime(activeSessionState.LastActiveAt);
    }
}
