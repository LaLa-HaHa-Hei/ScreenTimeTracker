using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteByHost;

public class GetWebsiteByHostHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsiteByHostQuery, ErrorOr<GetWebsiteByHostResponse>>
{
    public async ValueTask<ErrorOr<GetWebsiteByHostResponse>> Handle(
        GetWebsiteByHostQuery request,
        CancellationToken cancellationToken
    )
    {
        var website = await context
            .Websites.AsNoTracking()
            .FirstOrDefaultAsync(Website => Website.Host == request.Host, cancellationToken);

        if (website is null)
            return Error.NotFound(
                code: "Website.NotFound",
                description: "The website with the specified host was not found."
            );

        return new GetWebsiteByHostResponse(
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
