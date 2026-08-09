using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

public class GetWebsiteCategoryHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsiteCategoryQuery, ErrorOr<GetWebsiteCategoryResponse>>
{
    public async ValueTask<ErrorOr<GetWebsiteCategoryResponse>> Handle(
        GetWebsiteCategoryQuery request,
        CancellationToken cancellationToken
    )
    {
        var websiteCategory = await context
            .WebsiteCategories.AsNoTracking()
            .FirstOrDefaultAsync(
                websiteCategory => websiteCategory.Id == request.WebsiteCategoryId,
                cancellationToken
            );

        if (websiteCategory is null)
            return Error.NotFound(
                code: "WebsiteCategory.NotFound",
                description: "The website category with the specified ID was not found."
            );

        return new GetWebsiteCategoryResponse(
            websiteCategory.Id,
            websiteCategory.Name,
            websiteCategory.Color,
            websiteCategory.IconPath,
            websiteCategory.IconPathLastUpdatedAt,
            websiteCategory.IsSystem
        );
    }
}
