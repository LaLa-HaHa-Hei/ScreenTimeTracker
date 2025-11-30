using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessHourlyDistributionForDay;

public record GetProcessHourlyDistributionForDayQuery(
    Guid ProcessId,
    DateOnly Date) : IQuery<IDictionary<int, long>>
{
}