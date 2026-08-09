using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsage;

public record GetWebsiteUsageQuery(
    UsageGranularity Granularity,
    DateOnly StartDate,
    DateOnly EndDate,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<List<GetWebsiteUsageResponseItem>>;

public enum UsageGranularity
{
    Hour,
    Day,
}
