using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Apps;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.DeleteApp;

public class DeleteAppHandler(
    ScreenTimeDbContext context,
    ActiveAppUsageSessionStore activeSessionStore
) : IRequestHandler<DeleteAppCommand, ErrorOr<Deleted>>
{
    public async ValueTask<ErrorOr<Deleted>> Handle(
        DeleteAppCommand request,
        CancellationToken cancellationToken
    )
    {
        App? app = await context.Apps.FindAsync([request.AppId], cancellationToken);
        if (app is null)
            return Error.NotFound(
                code: "App.NotFound",
                description: "The app with the specified ID was not found."
            );

        if (app.IsSystem)
            return Error.Conflict(
                code: "App.SystemAppCannotBeDeleted",
                description: "System app cannot be deleted."
            );

        // 把所有 App 的数据都删除
        await context
            .AppUsageSessions.Where(log => log.AppId == request.AppId)
            .ExecuteDeleteAsync(cancellationToken);

        if (activeSessionStore.Current?.AppId == request.AppId)
            activeSessionStore.Current = null;

        context.Apps.Remove(app);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}
