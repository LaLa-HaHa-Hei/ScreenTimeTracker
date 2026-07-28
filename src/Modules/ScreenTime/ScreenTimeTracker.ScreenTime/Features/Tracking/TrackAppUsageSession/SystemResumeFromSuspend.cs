using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsageSession;

public record SystemResumeFromSuspendCommand(DateTime SuspendStartedAt) : IRequest;

public class SystemResumeFromSuspendHandler(
    ScreenTimeDbContext context,
    IActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : IRequestHandler<SystemResumeFromSuspendCommand>
{
    public async ValueTask<Unit> Handle(
        SystemResumeFromSuspendCommand request,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetLocalNow().DateTime;

        await activeSessionStore.StageSaveToAsync(context, now, cancellationToken);
        activeSessionStore.Current = null;

        // 删除已有数据中系统挂起开始到现在范围内数据
        var affectedSessions = await context
            .AppUsageSessions.Where(s => request.SuspendStartedAt <= s.EndTime)
            .ToListAsync(cancellationToken);

        foreach (var session in affectedSessions)
        {
            // 完全在系统挂起时间范围内
            if (request.SuspendStartedAt <= session.StartTime)
                context.AppUsageSessions.Remove(session);
            // 部分在系统挂起时间范围内
            else
                session.UpdateEndTime(request.SuspendStartedAt);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
