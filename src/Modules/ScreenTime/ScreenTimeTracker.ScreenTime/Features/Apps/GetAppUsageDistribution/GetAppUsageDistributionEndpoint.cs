using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageDistribution;

public class GetAppUsageDistributionEndpoint(IMediator mediator)
    : Endpoint<GetAppUsageDistributionRequest, GetAppUsageDistributionResponse>
{
    public override void Configure()
    {
        Get("usage/apps/distribution");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppUsageDistributionRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetAppUsageDistributionQuery(
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
