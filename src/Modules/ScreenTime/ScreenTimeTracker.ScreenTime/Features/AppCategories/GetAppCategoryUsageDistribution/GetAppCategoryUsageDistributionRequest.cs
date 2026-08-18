using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageDistribution;

public record GetAppCategoryUsageDistributionRequest(
    [property: QueryParam] DateOnly StartDate,
    [property: QueryParam] DateOnly EndDate,
    [property: QueryParam] string TimeZoneId,
    [property: QueryParam] int TopN = 10,
    [property: QueryParam] IEnumerable<Guid>? IncludedIds = null,
    [property: QueryParam] IEnumerable<Guid>? ExcludedIds = null
);
