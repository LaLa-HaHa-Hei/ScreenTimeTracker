using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.CreateWebsiteCategory;

public class CreateWebsiteCategoryHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<CreateWebsiteCategoryCommand, ErrorOr<CreateWebsiteCategoryResponse>>
{
    public async ValueTask<ErrorOr<CreateWebsiteCategoryResponse>> Handle(
        CreateWebsiteCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var exists = await context.WebsiteCategories.AnyAsync(
            x => x.Name == request.Name,
            cancellationToken
        );
        if (exists)
            return Error.Conflict(
                "WebsiteCategory.NameAlreadyExists",
                "An website category with the same name already exists."
            );

        WebsiteCategory websiteCategory = WebsiteCategory.Create(
            timeProvider.GetUtcNow(),
            request.Name,
            request.Color,
            request.IconPath
        );

        context.WebsiteCategories.Add(websiteCategory);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateWebsiteCategoryResponse(
            websiteCategory.Id,
            websiteCategory.Name,
            websiteCategory.Color,
            websiteCategory.IconPath,
            websiteCategory.IconPathLastUpdatedAt,
            websiteCategory.IsSystem
        );
    }
}
