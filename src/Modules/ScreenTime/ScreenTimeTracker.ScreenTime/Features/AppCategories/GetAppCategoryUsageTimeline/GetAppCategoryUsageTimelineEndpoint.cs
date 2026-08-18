using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageTimeline;

public class GetAppCategoryUsageTimelineEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoryUsageTimelineRequest, List<GetAppCategoryUsageTimelineResponseItem>>
{
    public override void Configure()
    {
        Get("usage/app-categories/timeline");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetAppCategoryUsageTimelineRequest req,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(
            new GetAppCategoryUsageTimelineQuery(
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
