using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetRankedProcessUsageForPeriodQuery(
#pragma warning restore MSG0005 // MediatorGenerator message warning
    DateOnly StartDate,
    DateOnly EndDate,
    int TopN = 10,
    IEnumerable<Guid>? ExcludedProcessIds = null) : IQuery<IEnumerable<ProcessUsageRankEntry>>
{
}