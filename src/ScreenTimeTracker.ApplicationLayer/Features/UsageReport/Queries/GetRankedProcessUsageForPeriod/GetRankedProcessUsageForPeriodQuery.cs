using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;

public record GetRankedProcessUsageForPeriodQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    int TopN = 10,
    IEnumerable<Guid>? ExcludedProcessIds = null) : IQuery<IEnumerable<ProcessUsageRankEntry>>
{
}