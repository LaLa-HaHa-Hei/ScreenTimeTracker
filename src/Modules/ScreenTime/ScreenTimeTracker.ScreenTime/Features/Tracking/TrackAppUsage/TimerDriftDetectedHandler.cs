using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public class TimerDriftDetectedHandler(
    ScreenTimeDbContext context,
    ActiveAppUsageSessionStore activeSessionStore,
    IForegroundWindowMonitor foregroundWindowMonitor,
    ForegroundWindowProcessor foregroundWindowProcessor,
    TimeProvider timeProvider
) : INotificationHandler<TimerDriftDetectedEvent>
{
    public async ValueTask Handle(
        TimerDriftDetectedEvent notification,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();
        var activeSession = activeSessionStore.Take();
        if (activeSession is not null && activeSession.StartTime < notification.DriftStartAt)
            await context.PersistActiveSessionAsync(
                activeSession,
                notification.DriftStartAt,
                cancellationToken
            );

        var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
        foregroundWindowProcessor.Enqueue(new ForegroundWindowChangedMessage(windowInfo, now));

        // 删除漂移开始到现在内的所有会话
        var affectedSessions = await context
            .AppUsageSessions.Where(s => notification.DriftStartAt <= s.EndTime)
            .ToListAsync(cancellationToken);

        foreach (var session in affectedSessions)
        {
            // 完全在漂移时间范围内
            if (notification.DriftStartAt <= session.UsagePeriod.Start)
                context.AppUsageSessions.Remove(session);
            // 部分在漂移时间范围内
            else
                session.UpdateEndTime(notification.DriftStartAt);
        }

        await context.SaveChangesAsync(cancellationToken);
        return;
    }
}
