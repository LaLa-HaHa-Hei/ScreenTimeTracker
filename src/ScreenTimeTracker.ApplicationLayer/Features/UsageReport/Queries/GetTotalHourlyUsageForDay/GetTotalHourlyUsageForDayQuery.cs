using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalHourlyUsageForDay;

public record GetTotalHourlyUsageForDayQuery(
    DateOnly Date,
    IEnumerable<Guid>? ExcludedProcessIds) : IQuery<IDictionary<int, long>>
{
}