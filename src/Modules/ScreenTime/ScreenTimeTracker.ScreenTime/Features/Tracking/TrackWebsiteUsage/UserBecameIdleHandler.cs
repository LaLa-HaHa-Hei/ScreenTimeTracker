using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;

public class UserBecameIdleHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore
) : INotificationHandler<UserBecameIdleEvent>
{
    public async ValueTask Handle(
        UserBecameIdleEvent notification,
        CancellationToken cancellationToken
    )
    {
        var activeSession = activeSessionStore.Take();
        if (activeSession is not null)
        {
            if (activeSession.StartTime < notification.IdleStartedAt)
                await context.PersistActiveSessionAsync(
                    activeSession,
                    notification.IdleStartedAt,
                    cancellationToken
                );
            else
            {
                var savedActiveSession = await context.WebsiteUsageSessions.FirstOrDefaultAsync(
                    x =>
                        x.WebsiteId == activeSession.WebsiteId
                        && x.StartTime == activeSession.StartTime,
                    cancellationToken
                );
                if (savedActiveSession is not null)
                    context.WebsiteUsageSessions.Remove(savedActiveSession);
            }
            activeSessionStore.Current = new ActiveWebsiteUsageSessionState(
                activeSession.WebsiteId,
                notification.IdleStartedAt,
                notification.IdleStartedAt
            );
        }

        // 删除已有数据中空闲开始以来的所有数据
        var affectedSessions = await context
            .WebsiteUsageSessions.Where(s => notification.IdleStartedAt <= s.EndTime)
            .ToListAsync(cancellationToken);

        foreach (var session in affectedSessions)
        {
            // 完全在空闲时间范围内
            if (notification.IdleStartedAt <= session.UsagePeriod.Start)
                context.WebsiteUsageSessions.Remove(session);
            // 部分在空闲时间范围内
            else
                session.UpdateEndTime(notification.IdleStartedAt);
        }

        await context.SaveChangesAsync(cancellationToken);
        return;
    }
}
