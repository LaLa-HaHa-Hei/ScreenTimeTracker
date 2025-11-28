using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessDailyDistributionForPeriod;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetProcessDailyDistributionForPeriodQuery(
#pragma warning restore MSG0005 // MediatorGenerator message warning
    Guid ProcessId,
    DateOnly StartDate,
    DateOnly EndDate) : IQuery<IDictionary<DateOnly, long>>
{
}