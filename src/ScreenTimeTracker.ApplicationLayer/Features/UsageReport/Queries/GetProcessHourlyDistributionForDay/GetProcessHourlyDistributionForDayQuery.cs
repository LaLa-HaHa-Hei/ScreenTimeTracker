using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessHourlyDistributionForDay;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetProcessHourlyDistributionForDayQuery(
#pragma warning restore MSG0005 // MediatorGenerator message warning
    Guid ProcessId,
    DateOnly Date) : IQuery<IDictionary<int, long>>
{
}