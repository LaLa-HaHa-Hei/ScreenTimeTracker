using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteCategories;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.DeleteWebsiteCategory;

public class DeleteWebsiteCategoryHandler(ScreenTimeDbContext context)
    : IRequestHandler<DeleteWebsiteCategoryCommand, ErrorOr<Deleted>>
{
    public async ValueTask<ErrorOr<Deleted>> Handle(
        DeleteWebsiteCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        WebsiteCategory? websiteCategory = await context.WebsiteCategories.FindAsync(
            [request.WebsiteCategoryId],
            cancellationToken
        );

        if (websiteCategory is null)
            return Error.NotFound(
                code: "WebsiteCategory.NotFound",
                description: "The website category with the specified ID was not found."
            );

        if (websiteCategory.IsSystem)
            return Error.Conflict(
                code: "WebsiteCategory.SystemWebsiteCategoryCannotBeDeleted",
                description: "System website categories cannot be deleted."
            );

        // 把所有这个类别的 Website 都设置为默认类别
        await context
            .Websites.Where(website => website.WebsiteCategoryId == request.WebsiteCategoryId)
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(
                        website => website.WebsiteCategoryId,
                        WebsiteCategory.UncategorizedId
                    ),
                cancellationToken
            );

        context.WebsiteCategories.Remove(websiteCategory);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}
