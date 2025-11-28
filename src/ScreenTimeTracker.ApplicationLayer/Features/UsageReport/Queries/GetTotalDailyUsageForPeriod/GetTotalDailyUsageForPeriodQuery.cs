using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalDailyUsageForPeriod;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetTotalDailyUsageForPeriodQuery(
#pragma warning restore MSG0005 // MediatorGenerator message warning
    DateOnly StartDate,
    DateOnly EndDate,
    IEnumerable<Guid>? ExcludedProcessIds = null) : IQuery<IDictionary<DateOnly, long>>
{
}