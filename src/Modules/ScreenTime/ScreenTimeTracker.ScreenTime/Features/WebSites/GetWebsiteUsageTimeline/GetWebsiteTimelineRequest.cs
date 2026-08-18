using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageTimeline;

public record GetWebsiteUsageTimelineRequest(
    [property: QueryParam] DateOnly StartDate,
    [property: QueryParam] DateOnly EndDate,
    [property: QueryParam] string TimeZoneId,
    [property: QueryParam] IEnumerable<Guid>? IncludedIds = null,
    [property: QueryParam] IEnumerable<Guid>? ExcludedIds = null
);
