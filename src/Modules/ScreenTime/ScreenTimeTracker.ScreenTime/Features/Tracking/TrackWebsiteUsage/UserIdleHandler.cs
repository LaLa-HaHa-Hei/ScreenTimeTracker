using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;

public record UserIdleCommand(DateTimeOffset IdleStartedAt) : IRequest;

public class UserIdleHandlerHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : IRequestHandler<UserIdleCommand>
{
    public async ValueTask<Unit> Handle(
        UserIdleCommand request,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetUtcNow();

        if (activeSessionStore.Current is not null)
        {
            await activeSessionStore.Current.PersistSessionAsync(context, cancellationToken);
            activeSessionStore.Current = null;
        }

        // 修正已有数据中空闲开始到现在范围内数据为空闲
        var affectedSessions = await context
            .WebsiteUsageSessions.Where(s => request.IdleStartedAt <= s.EndTime)
            .ToListAsync(cancellationToken);

        foreach (var session in affectedSessions)
        {
            // 完全在空闲时间范围内
            if (request.IdleStartedAt <= session.StartTime)
                context.WebsiteUsageSessions.Remove(session);
            // 部分在空闲时间范围内
            else
                session.UpdateEndTime(request.IdleStartedAt);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
