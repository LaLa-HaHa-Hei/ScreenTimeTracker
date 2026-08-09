using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageTimeline;

public record GetWebsiteCategoryUsageTimelineQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<List<GetWebsiteCategoryUsageTimelineResponseItem>>;
