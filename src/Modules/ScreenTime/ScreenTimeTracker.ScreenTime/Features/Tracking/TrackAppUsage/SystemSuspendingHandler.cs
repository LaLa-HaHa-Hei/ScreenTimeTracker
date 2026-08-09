using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public record SystemSuspendingCommand : IRequest;

public class SystemSuspendingHandlerHandler(
    ActiveAppUsageSessionStore activeSessionStore,
    ScreenTimeDbContext context,
    TimeProvider timeProvider
) : IRequestHandler<SystemSuspendingCommand>
{
    public async ValueTask<Unit> Handle(
        SystemSuspendingCommand request,
        CancellationToken cancellationToken
    )
    {
        if (activeSessionStore.Current is null)
            return Unit.Value;

        var now = timeProvider.GetUtcNow();
        await activeSessionStore.Current.PersistSessionAsync(context, now, cancellationToken);
        activeSessionStore.Current = null;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
