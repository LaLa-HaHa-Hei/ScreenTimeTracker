using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalDailyUsageForPeriod;

public record GetTotalDailyUsageForPeriodQuery(
    DateOnly StartDate,
    DateOnly EndDate,
    IEnumerable<Guid>? ExcludedProcessIds = null) : IQuery<IDictionary<DateOnly, long>>
{
}