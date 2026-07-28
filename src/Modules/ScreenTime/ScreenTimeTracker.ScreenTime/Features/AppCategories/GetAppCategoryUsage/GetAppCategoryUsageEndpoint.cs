using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsage;

public class GetAppCategoryUsageEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoryUsageRequest, List<GetAppCategoryUsageResponseItem>>
{
    public override void Configure()
    {
        Get("usage/app-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppCategoryUsageRequest req, CancellationToken ct)
    {
        var response = await mediator.Send(
            new GetAppCategoryUsageQuery(
                req.Granularity,
                req.StartDate,
                req.EndDate,
                req.IncludedIds,
                req.ExcludedIds
            ),
            ct
        );
        await Send.OkAsync(response, ct);
    }
}
