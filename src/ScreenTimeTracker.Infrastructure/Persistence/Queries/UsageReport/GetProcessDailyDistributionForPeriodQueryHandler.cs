using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessDailyDistributionForPeriod;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.UsageReport;

public class GetProcessDailyDistributionForPeriodQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetProcessDailyDistributionForPeriodQuery, IDictionary<DateOnly, long>>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<IDictionary<DateOnly, long>> Handle(GetProcessDailyDistributionForPeriodQuery query, CancellationToken cancellationToken)
    {
        var start = query.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var end = query.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).AddDays(1);

        var summaryQuery = _dbContext.HourlySummaries
            .AsNoTracking()
            .Where(h => h.ProcessInfoEntityId == query.ProcessId && h.Hour >= start && h.Hour < end)
            .Select(h => new { Date = h.Hour.Date, Milliseconds = (long)h.TotalDurationMilliseconds });

        var intervalQuery = _dbContext.ActivityIntervals
            .AsNoTracking()
            .Where(a => a.ProcessInfoEntityId == query.ProcessId && a.Timestamp >= start && a.Timestamp < end)
            .Select(a => new { Date = a.Timestamp.Date, Milliseconds = (long)a.DurationMilliseconds });

        var dailyUsage = await summaryQuery.Concat(intervalQuery)
            .GroupBy(u => u.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalMilliseconds = g.Sum(u => u.Milliseconds)
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return dailyUsage.ToDictionary(
            d => DateOnly.FromDateTime(d.Date),
            d => d.TotalMilliseconds / 1000
        );
    }
}
