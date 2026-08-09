using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsage;

public class GetWebsiteUsageEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteUsageRequest, List<GetWebsiteUsageResponseItem>>
{
    public override void Configure()
    {
        Get("usage/websites");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWebsiteUsageRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetWebsiteUsageQuery(
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
