using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalHourlyUsageForDay;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.UsageReport;

public class GetTotalHourlyUsageForDayQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetTotalHourlyUsageForDayQuery, IDictionary<int, long>>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<IDictionary<int, long>> Handle(GetTotalHourlyUsageForDayQuery query, CancellationToken cancellationToken)
    {
        var start = query.Date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var end = start.AddDays(1);

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
        var hourlyUsage = await summaryQuery
            .Select(h => new { Hour = h.Hour.Hour, Milliseconds = (long)h.TotalDurationMilliseconds })
            .Concat(intervalQuery.Select(a => new { Hour = a.Timestamp.Hour, Milliseconds = (long)a.DurationMilliseconds }))
            .GroupBy(u => u.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                TotalMilliseconds = g.Sum(u => u.Milliseconds)
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return hourlyUsage.ToDictionary(
            h => h.Hour,
            h => h.TotalMilliseconds / 1000
        );
    }
}
