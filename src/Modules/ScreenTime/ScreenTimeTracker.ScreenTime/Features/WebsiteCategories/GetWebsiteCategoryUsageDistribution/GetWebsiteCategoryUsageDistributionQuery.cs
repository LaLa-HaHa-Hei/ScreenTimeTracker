using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageDistribution;

public record GetWebsiteCategoryUsageDistributionQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    int TopN = 10,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<GetWebsiteCategoryUsageDistributionResponse>;
