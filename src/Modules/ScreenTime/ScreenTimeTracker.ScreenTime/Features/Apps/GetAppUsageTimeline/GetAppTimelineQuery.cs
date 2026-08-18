using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageTimeline;

public record GetAppUsageTimelineQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeZoneInfo TimeZoneInfo,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<List<GetAppUsageTimelineResponseItem>>;
