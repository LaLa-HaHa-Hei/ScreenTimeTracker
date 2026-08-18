using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public class GetWebsiteUsageDistributionEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteUsageDistributionRequest, GetWebsiteUsageDistributionResponse>
{
    public override void Configure()
    {
        Get("usage/websites/distribution");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetWebsiteUsageDistributionRequest req,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(
            new GetWebsiteUsageDistributionQuery(
                req.StartDate,
                req.EndDate,
                TimeZoneInfo.FindSystemTimeZoneById(req.TimeZoneId),
                req.TopN,
                req.IncludedIds,
                req.ExcludedIds
            ),
            ct
        );
        await Send.OkAsync(result, ct);
    }
}
