using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalDailyUsageForPeriod;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.UsageReport;

public class GetTotalDailyUsageForPeriodQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetTotalDailyUsageForPeriodQuery, IDictionary<DateOnly, long>>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<IDictionary<DateOnly, long>> Handle(GetTotalDailyUsageForPeriodQuery query, CancellationToken cancellationToken)
    {
        var start = query.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var end = query.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).AddDays(1);

        // 定义查询源
        var summaryQuery = _dbContext.HourlySummaries
            .AsNoTracking()
            .Where(h => h.Hour >= start && h.Hour < end);

        var intervalQuery = _dbContext.ActivityIntervals
            .AsNoTracking()
            .Where(a => a.Timestamp >= start && a.Timestamp < end);

        // 应用排除筛选
        if (query.ExcludedProcessIds?.Any() == true)
        {
            summaryQuery = summaryQuery.Where(s => !query.ExcludedProcessIds.Contains(s.ProcessInfoEntityId));
            intervalQuery = intervalQuery.Where(i => !query.ExcludedProcessIds.Contains(i.ProcessInfoEntityId));
        }

        // 投影、合并、分组和求和
        var dailyUsage = await summaryQuery
            .Select(h => new { Date = h.Hour.Date, Milliseconds = (long)h.TotalDurationMilliseconds })
            .Concat(intervalQuery.Select(a => new { Date = a.Timestamp.Date, Milliseconds = (long)a.DurationMilliseconds }))
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
