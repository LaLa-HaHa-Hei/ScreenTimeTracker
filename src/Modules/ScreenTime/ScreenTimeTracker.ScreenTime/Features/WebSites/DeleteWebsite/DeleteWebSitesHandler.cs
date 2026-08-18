using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.DeleteWebsite;

public class DeleteWebsiteHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore
) : IRequestHandler<DeleteWebsiteCommand, ErrorOr<Deleted>>
{
    public async ValueTask<ErrorOr<Deleted>> Handle(
        DeleteWebsiteCommand request,
        CancellationToken cancellationToken
    )
    {
        Website? website = await context.Websites.FindAsync([request.WebsiteId], cancellationToken);
        if (website is null)
            return Error.NotFound(
                code: "Website.NotFound",
                description: "The website with the specified ID was not found."
            );

        if (website.IsSystem)
            return Error.Conflict(
                code: "Website.SystemWebsiteCannotBeDeleted",
                description: "System website cannot be deleted."
            );

        // 把所有 Website 的数据都删除
        await context
            .WebsiteUsageSessions.Where(log => log.WebsiteId == request.WebsiteId)
            .ExecuteDeleteAsync(cancellationToken);

        if (activeSessionStore.Current?.WebsiteId == request.WebsiteId)
            activeSessionStore.Current = null;

        context.Websites.Remove(website);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}
