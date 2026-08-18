using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageDistribution;

public record GetWebsiteCategoryUsageDistributionQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeZoneInfo TimeZoneInfo,
    int TopN = 10,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<GetWebsiteCategoryUsageDistributionResponse>;
