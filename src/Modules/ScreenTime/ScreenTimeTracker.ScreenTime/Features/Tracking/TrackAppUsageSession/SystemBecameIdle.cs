using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsageSession;

public record SystemBecameIdleCommand(DateTime IdleStartedAt) : IRequest;

public class SystemBecameIdleHandler(
    ScreenTimeDbContext context,
    IActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : IRequestHandler<SystemBecameIdleCommand>
{
    public async ValueTask<Unit> Handle(
        SystemBecameIdleCommand request,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetLocalNow().DateTime;

        await activeSessionStore.StageSaveToAsync(context, now, cancellationToken);
        activeSessionStore.Current = new ActiveAppUsageSessionState(App.IdleAppId, now);

        // 修正已有数据中空闲开始到现在范围内数据为空闲
        var affectedSessions = await context
            .AppUsageSessions.Where(s => request.IdleStartedAt <= s.EndTime)
            .ToListAsync(cancellationToken);

        foreach (var session in affectedSessions)
        {
            // 完全在空闲时间范围内
            if (request.IdleStartedAt <= session.StartTime)
                session.MarkAsIdle(App.IdleAppId);
            // 部分在空闲时间范围内
            else
            {
                var idlePartSession = AppUsageSession.Create(
                    App.IdleAppId,
                    request.IdleStartedAt,
                    session.EndTime
                );
                context.AppUsageSessions.Add(idlePartSession);
                session.UpdateEndTime(request.IdleStartedAt);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
