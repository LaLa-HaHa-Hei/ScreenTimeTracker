using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public record GetWebsiteUsageDistributionRequest(
    [property: QueryParam] DateOnly StartDate,
    [property: QueryParam] DateOnly EndDate,
    [property: QueryParam] int TopN = 10,
    [property: QueryParam] IEnumerable<Guid>? ExcludedIds = null
);
