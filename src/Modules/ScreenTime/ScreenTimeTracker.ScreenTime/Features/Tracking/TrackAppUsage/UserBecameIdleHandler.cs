using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Apps;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public class UserBecameIdleHandler(
    ScreenTimeDbContext context,
    ActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : INotificationHandler<UserBecameIdleEvent>
{
    public async ValueTask Handle(
        UserBecameIdleEvent notification,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();

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
                var savedActiveSession = await context.AppUsageSessions.FirstOrDefaultAsync(
                    x => x.AppId == activeSession.AppId && x.StartTime == activeSession.StartTime,
                    cancellationToken
                );
                if (savedActiveSession is not null)
                    context.AppUsageSessions.Remove(savedActiveSession);
            }
            activeSessionStore.Current = new ActiveAppUsageSessionState(
                App.IdleAppId,
                notification.IdleStartedAt
            );
        }

        // 修正已有数据中空闲开始到现在范围内数据为空闲
        var affectedSessions = await context
            .AppUsageSessions.Where(s => notification.IdleStartedAt <= s.EndTime)
            .ToListAsync(cancellationToken);

        foreach (var session in affectedSessions)
        {
            // 完全在空闲时间范围内
            if (notification.IdleStartedAt <= session.UsagePeriod.Start)
                session.MarkAsIdle(App.IdleAppId);
            // 部分在空闲时间范围内
            else
            {
                var idlePartSession = AppUsageSession.Create(
                    App.IdleAppId,
                    new TimeRange(notification.IdleStartedAt, session.UsagePeriod.End)
                );
                context.AppUsageSessions.Add(idlePartSession);
                session.UpdateEndTime(notification.IdleStartedAt);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return;
    }
}
