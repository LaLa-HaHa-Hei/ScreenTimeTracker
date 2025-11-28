using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessHourlyDistributionForDay;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.UsageReport;

public class GetProcessHourlyDistributionForDayQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetProcessHourlyDistributionForDayQuery, IDictionary<int, long>>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<IDictionary<int, long>> Handle(GetProcessHourlyDistributionForDayQuery query, CancellationToken cancellationToken)
    {
        var start = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var end = start.AddDays(1);

        var summaryQuery = _dbContext.HourlySummaries
            .AsNoTracking()
            .Where(h => h.ProcessInfoEntityId == query.ProcessId && h.Hour >= start && h.Hour < end)
            .Select(h => new { Hour = h.Hour.Hour, Milliseconds = (long)h.TotalDurationMilliseconds });

        var intervalQuery = _dbContext.ActivityIntervals
            .AsNoTracking()
            .Where(a => a.ProcessInfoEntityId == query.ProcessId && a.Timestamp >= start && a.Timestamp < end)
            .Select(a => new { Hour = a.Timestamp.Hour, Milliseconds = (long)a.DurationMilliseconds });

        var hourlyUsage = await summaryQuery.Concat(intervalQuery)
            .GroupBy(u => u.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                TotalMilliseconds = g.Sum(u => u.Milliseconds)
            })
            .ToListAsync();

        return hourlyUsage.ToDictionary(
            h => h.Hour,
            h => h.TotalMilliseconds / 1000
        );
    }
}
