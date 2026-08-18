using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsage;

public record GetWebsiteUsageRequest(
    [property: QueryParam] UsageGranularity Granularity,
    [property: QueryParam] DateOnly StartDate,
    [property: QueryParam] DateOnly EndDate,
    [property: QueryParam] string TimeZoneId,
    [property: QueryParam] IEnumerable<Guid>? IncludedIds = null,
    [property: QueryParam] IEnumerable<Guid>? ExcludedIds = null
);
