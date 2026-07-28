using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.DeleteAppCategory;

public class DeleteAppCategoryHandler(ScreenTimeDbContext context)
    : IRequestHandler<DeleteAppCategoryCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteAppCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        AppCategory? appCategory = await context.AppCategories.FindAsync(
            [request.AppCategoryId],
            cancellationToken
        );
        if (appCategory is null)
            return Unit.Value;

        if (appCategory.IsSystem)
            throw new InvalidOperationException("Cannot delete a system app category.");

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
        return Unit.Value;
    }
}
