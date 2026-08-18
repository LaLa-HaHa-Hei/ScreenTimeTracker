using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.DeleteAppCategory;

public class DeleteAppCategoryHandler(ScreenTimeDbContext context)
    : IRequestHandler<DeleteAppCategoryCommand, ErrorOr<Deleted>>
{
    public async ValueTask<ErrorOr<Deleted>> Handle(
        DeleteAppCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        AppCategory? appCategory = await context.AppCategories.FindAsync(
            [request.AppCategoryId],
            cancellationToken
        );

        if (appCategory is null)
            return Error.NotFound(
                code: "AppCategory.NotFound",
                description: "The app category with the specified ID was not found."
            );

        if (appCategory.IsSystem)
            return Error.Conflict(
                code: "AppCategory.SystemAppCategoryCannotBeDeleted",
                description: "System app categories cannot be deleted."
            );

        // 把所有这个类别的 App 都设置为默认类别
        await context
            .Apps.Where(app => app.AppCategoryId == request.AppCategoryId)
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(app => app.AppCategoryId, AppCategory.UncategorizedId),
                cancellationToken
            );

        context.AppCategories.Remove(appCategory);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Deleted;
    }
}
