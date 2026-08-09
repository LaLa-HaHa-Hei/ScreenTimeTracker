using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsite;

public class GetWebsiteHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsiteQuery, ErrorOr<GetWebsiteResponse>>
{
    public async ValueTask<ErrorOr<GetWebsiteResponse>> Handle(
        GetWebsiteQuery request,
        CancellationToken cancellationToken
    )
    {
        var website = await context
            .Websites.AsNoTracking()
            .FirstOrDefaultAsync(Website => Website.Id == request.WebsiteId, cancellationToken);

        if (website is null)
            return Error.NotFound(
                code: "Website.NotFound",
                description: "The website with the specified ID was not found."
            );

        return new GetWebsiteResponse(
            website.Id,
            website.Name,
            website.Color,
            website.Host,
            website.AllowMetadataAutoRefresh,
            website.MetadataLastRefreshedAt,
            website.CategoryId,
            website.IconPath,
            website.IconPathLastUpdatedAt,
            website.IsSystem
        );
    }
}
