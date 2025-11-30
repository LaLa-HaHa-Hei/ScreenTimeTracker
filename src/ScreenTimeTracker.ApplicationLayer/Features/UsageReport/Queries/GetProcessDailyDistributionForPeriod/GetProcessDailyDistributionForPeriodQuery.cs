using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessDailyDistributionForPeriod;

public record GetProcessDailyDistributionForPeriodQuery(
    Guid ProcessId,
    DateOnly StartDate,
    DateOnly EndDate) : IQuery<IDictionary<DateOnly, long>>
{
}