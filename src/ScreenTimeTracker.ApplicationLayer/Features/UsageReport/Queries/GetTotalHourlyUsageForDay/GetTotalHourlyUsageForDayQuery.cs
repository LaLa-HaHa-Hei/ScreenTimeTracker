using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalHourlyUsageForDay;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetTotalHourlyUsageForDayQuery(
#pragma warning restore MSG0005 // MediatorGenerator message warning
    DateOnly Date,
    IEnumerable<Guid>? ExcludedProcessIds) : IQuery<IDictionary<int, long>>
{
}