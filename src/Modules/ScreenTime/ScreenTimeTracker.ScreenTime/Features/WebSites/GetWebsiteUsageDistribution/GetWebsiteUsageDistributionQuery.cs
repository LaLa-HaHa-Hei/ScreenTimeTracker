using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public record GetWebsiteUsageDistributionQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeZoneInfo TimeZoneInfo,
    int TopN = 10,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<GetWebsiteUsageDistributionResponse>;
