using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageTimeline;

public class GetAppUsageTimelineEndpoint(IMediator mediator)
    : Endpoint<GetAppUsageTimelineRequest, List<GetAppUsageTimelineResponseItem>>
{
    public override void Configure()
    {
        Get("usage/apps/timeline");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppUsageTimelineRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetAppUsageTimelineQuery(
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
