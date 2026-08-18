using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageDistribution;

public record GetAppUsageDistributionQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeZoneInfo TimeZoneInfo,
    int TopN = 10,
    IEnumerable<Guid>? IncludedIds = null,
    IEnumerable<Guid>? ExcludedIds = null
) : IRequest<GetAppUsageDistributionResponse>;
