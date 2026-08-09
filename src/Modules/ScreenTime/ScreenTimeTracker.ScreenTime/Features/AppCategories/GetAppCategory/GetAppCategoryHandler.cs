using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public class GetAppCategoryHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppCategoryQuery, ErrorOr<GetAppCategoryResponse>>
{
    public async ValueTask<ErrorOr<GetAppCategoryResponse>> Handle(
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

        if (appCategory is null)
            return Error.NotFound(
                code: "AppCategory.NotFound",
                description: "The app category with the specified ID was not found."
            );

        return new GetAppCategoryResponse(
            appCategory.Id,
            appCategory.Name,
            appCategory.Color,
            appCategory.IconPath,
            appCategory.IconPathLastUpdatedAt,
            appCategory.IsSystem
        );
    }
}
