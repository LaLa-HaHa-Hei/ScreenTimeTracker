using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;

namespace ScreenTimeTracker.ApplicationLayer.Interfaces;

public interface IUsageReportReadService
{
    Task<IDictionary<DateOnly, long>> GetProcessDailyDistributionForPeriod(
        Guid processId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    Task<IDictionary<int, long>> GetProcessHourlyDistributionForDay(Guid processId, DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ProcessUsageRankEntry>> GetRankedProcessUsageForPeriod(
        DateOnly startDate, DateOnly endDate, int topN = 10,
        IEnumerable<Guid>? excludedProcessIds = null, CancellationToken cancellationToken = default);

    Task<IDictionary<DateOnly, long>> GetTotalDailyUsageForPeriod(
        DateOnly startDate, DateOnly endDate,
        IEnumerable<Guid>? excludedProcessIds = null, CancellationToken cancellationToken = default);

    Task<IDictionary<int, long>> GetTotalHourlyUsageForDay(DateOnly date,
        IEnumerable<Guid>? excludedProcessIds = null, CancellationToken cancellationToken = default);
}