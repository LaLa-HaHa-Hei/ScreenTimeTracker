using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public class GetAppCategoryHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppCategoryQuery, GetAppCategoryResponse?>
{
    public async ValueTask<GetAppCategoryResponse?> Handle(
        GetAppCategoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var appCategory = await context
            .AppCategories.AsNoTracking()
            .FirstOrDefaultAsync(
                appCategory => appCategory.Id == request.AppCategoryId,
                cancellationToken
            );
        return appCategory is null
            ? null
            : new GetAppCategoryResponse(
                appCategory.Id,
                appCategory.Name,
                appCategory.Color,
                appCategory.IconPath,
                appCategory.IconPathLastUpdatedAt,
                appCategory.IsSystem
            );
    }
}
