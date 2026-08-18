using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageDistribution;

public class GetAppCategoryUsageDistributionEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoryUsageDistributionRequest, GetAppCategoryUsageDistributionResponse>
{
    public override void Configure()
    {
        Get("usage/app-categories/distribution");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(
        GetAppCategoryUsageDistributionRequest req,
        CancellationToken ct
    )
    {
        var result = await mediator.Send(
            new GetAppCategoryUsageDistributionQuery(
                req.StartDate,
                req.EndDate,
                TimeZoneInfo.FindSystemTimeZoneById(req.TimeZoneId),
                req.TopN,
                req.ExcludedIds
            ),
            ct
        );
        await Send.OkAsync(result, ct);
    }
}
