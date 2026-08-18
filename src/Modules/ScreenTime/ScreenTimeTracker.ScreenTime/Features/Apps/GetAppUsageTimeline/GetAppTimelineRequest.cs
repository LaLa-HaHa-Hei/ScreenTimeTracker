using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageTimeline;

public record GetAppUsageTimelineRequest(
    [property: QueryParam] DateOnly StartDate,
    [property: QueryParam] DateOnly EndDate,
    [property: QueryParam] string TimeZoneId,
    [property: QueryParam] IEnumerable<Guid>? IncludedIds = null,
    [property: QueryParam] IEnumerable<Guid>? ExcludedIds = null
);
