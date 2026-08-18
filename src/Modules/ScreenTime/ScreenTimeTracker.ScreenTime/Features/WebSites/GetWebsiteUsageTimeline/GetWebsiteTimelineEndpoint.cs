using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageTimeline;

public class GetWebsiteUsageTimelineEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteUsageTimelineRequest, List<GetWebsiteUsageTimelineResponseItem>>
{
    public override void Configure()
    {
        Get("usage/websites/timeline");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWebsiteUsageTimelineRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new GetWebsiteUsageTimelineQuery(
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
