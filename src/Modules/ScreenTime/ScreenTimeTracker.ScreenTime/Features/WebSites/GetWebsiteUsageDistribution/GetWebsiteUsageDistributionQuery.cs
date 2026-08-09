using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public record GetWebsiteUsageDistributionQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    int TopN = 10,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<GetWebsiteUsageDistributionResponse>;
