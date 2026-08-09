using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage.RecordActivity;

public class RecordActivityHandler(
    TimeProvider timeProvider,
    ActiveWebsiteUsageSessionStore activeSessionStore,
    SystemSuspendStore systemSuspendedStore,
    UserIdleStore userIdleStore,
    ScreenTimeDbContext context
) : IRequestHandler<RecordActivityCommand>
{
    // 允许的网络抖动与漂移容忍度
    private static readonly TimeSpan _allowedNetworkJitter = TimeSpan.FromSeconds(5);

    public async ValueTask<Unit> Handle(
        RecordActivityCommand request,
        CancellationToken cancellationToken
    )
    {
        if (systemSuspendedStore.Current.IsSystemSuspendActive || userIdleStore.Current.IsUserIdle)
            return Unit.Value;

        var now = timeProvider.GetUtcNow();
        var website = await context.Websites.FirstOrDefaultAsync(
            w => w.Host == request.Host,
            cancellationToken
        );

        if (website is null)
        {
            website = Website.CreateDiscovered(now - request.Duration, request.Name, request.Host);
            context.Websites.Add(website);
        }

        if (activeSessionStore.Current is null)
        {
            activeSessionStore.Current = new(website.Id, now - request.Duration, now);
        }
        // 当前活跃的是这个网站并且和已有的衔接后误差不超过一定范围
        else if (
            activeSessionStore.Current.WebsiteId == website.Id
            && (activeSessionStore.Current.LastActiveAt + request.Duration - now).Duration()
                <= _allowedNetworkJitter
        )
            activeSessionStore.Current = activeSessionStore.Current with { LastActiveAt = now };
        else
        {
            await activeSessionStore.Current.PersistSessionAsync(context, cancellationToken);
            activeSessionStore.Current = new(website.Id, now - request.Duration, now);
        }

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
