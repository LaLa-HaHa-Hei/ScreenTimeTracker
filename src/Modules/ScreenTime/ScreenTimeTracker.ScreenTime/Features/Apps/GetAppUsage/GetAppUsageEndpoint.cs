using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsage;

public class GetAppUsageEndpoint(IMediator mediator)
    : Endpoint<GetAppUsageRequest, List<GetAppUsageResponseItem>>
{
    public override void Configure()
    {
        Get("usage/apps");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppUsageRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetAppUsageQuery(
                req.Granularity,
                req.StartDate,
                req.EndDate,
                TimeZoneInfo.FindSystemTimeZoneById(req.TimeZoneId),
                req.IncludedIds,
                req.ExcludedIds
            ),
            ct
        );
        await Send.OkAsync(result, ct);
    }
}
