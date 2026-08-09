using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageTimeline;

public record GetWebsiteUsageTimelineQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<List<GetWebsiteUsageTimelineResponseItem>>;
