using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsage;

public class GetWebsiteCategoryUsageEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteCategoryUsageRequest, List<GetWebsiteCategoryUsageResponseItem>>
{
    public override void Configure()
    {
        Get("usage/website-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWebsiteCategoryUsageRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetWebsiteCategoryUsageQuery(
                req.Granularity,
                req.StartDate,
                req.EndDate,
                req.IncludedIds,
                req.ExcludedIds
            ),
            ct
        );
        await Send.OkAsync(result, ct);
    }
}
