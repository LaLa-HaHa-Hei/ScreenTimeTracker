using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageDistribution;

public class GetWebsiteCategoryUsageDistributionEndpoint(IMediator mediator)
    : Endpoint<
        GetWebsiteCategoryUsageDistributionRequest,
        GetWebsiteCategoryUsageDistributionResponse
    >
{
    public override void Configure()
    {
        Get("usage/website-categories/distribution");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetWebsiteCategoryUsageDistributionRequest req,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(
            new GetWebsiteCategoryUsageDistributionQuery(
                req.StartDate,
                req.EndDate,
                req.TopN,
                req.ExcludedIds
            ),
            ct
        );
        await Send.OkAsync(result, ct);
    }
}
