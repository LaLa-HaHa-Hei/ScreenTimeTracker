using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageTimeline;

public record GetAppCategoryUsageTimelineQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeZoneInfo TimeZoneInfo,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<List<GetAppCategoryUsageTimelineResponseItem>>;
