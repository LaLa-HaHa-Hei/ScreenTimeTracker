using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsage;

public record GetWebsiteCategoryUsageQuery(
    UsageGranularity Granularity,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeZoneInfo TimeZoneInfo,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<List<GetWebsiteCategoryUsageResponseItem>>;

public enum UsageGranularity
{
    Hour,
    Day,
}
