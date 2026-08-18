using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageTimeline;

public class GetWebsiteCategoryUsageTimelineEndpoint(IMediator mediator)
    : Endpoint<
        GetWebsiteCategoryUsageTimelineRequest,
        List<GetWebsiteCategoryUsageTimelineResponseItem>
    >
{
    public override void Configure()
    {
        Get("usage/website-categories/timeline");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetWebsiteCategoryUsageTimelineRequest req,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(
            new GetWebsiteCategoryUsageTimelineQuery(
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
