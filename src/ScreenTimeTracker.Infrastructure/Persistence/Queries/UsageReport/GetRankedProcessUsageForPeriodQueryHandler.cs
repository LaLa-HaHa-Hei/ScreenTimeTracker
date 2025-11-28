using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.UsageReport;

public class GetRankedProcessUsageForPeriodQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetRankedProcessUsageForPeriodQuery, IEnumerable<ProcessUsageRankEntry>>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<IEnumerable<ProcessUsageRankEntry>> Handle(GetRankedProcessUsageForPeriodQuery query, CancellationToken cancellationToken)
    {
        var start = query.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var end = query.EndDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).AddDays(1);

        var summaryQuery = _dbContext.HourlySummaries
            .AsNoTracking()
            .Where(h => h.Hour >= start && h.Hour < end);
        var intervalQuery = _dbContext.ActivityIntervals
            .AsNoTracking()
            .Where(a => a.Timestamp >= start && a.Timestamp < end);

        if (query.ExcludedProcessIds?.Any() == true)
        {
            summaryQuery = summaryQuery.Where(s => !query.ExcludedProcessIds.Contains(s.ProcessInfoEntityId));
            intervalQuery = intervalQuery.Where(i => !query.ExcludedProcessIds.Contains(i.ProcessInfoEntityId));
        }

        // 投影为统一结构
        var projectedSummaryQuery = summaryQuery.Select(h => new { ProcessId = h.ProcessInfoEntityId, Duration = (long)h.TotalDurationMilliseconds });
        var projectedIntervalQuery = intervalQuery.Select(a => new { ProcessId = a.ProcessInfoEntityId, Duration = (long)a.DurationMilliseconds });

        // 合并两个 IQueryable，然后进行分组和求和。
        // 查询尚未执行。
        var combinedUsageQuery = projectedSummaryQuery.Concat(projectedIntervalQuery)
            .GroupBy(u => u.ProcessId)
            .Select(g => new
            {
                ProcessId = g.Key,
                TotalDurationMilliseconds = g.Sum(u => u.Duration)
            });

        // 执行查询，一次性从数据库获取所有进程的总时长
        var allUsage = await combinedUsageQuery.ToListAsync(cancellationToken: cancellationToken);

        if (allUsage.Count == 0)
            return [];

        // 在内存中计算总时长
        long totalMilliseconds = allUsage.Sum(p => p.TotalDurationMilliseconds);
        if (totalMilliseconds == 0)
            return [];

        // 在内存中排序并获取 Top N
        var topProcessUsage = allUsage
            .OrderByDescending(p => p.TotalDurationMilliseconds)
            .Take(query.TopN)
            .ToArray();

        var topProcessIds = topProcessUsage.Select(p => p.ProcessId).ToList();

        // 第二次数据库查询：获取 Top N 进程的详细信息
        var processInfos = await _dbContext.ProcessInfos
            .AsNoTracking()
            .Where(p => topProcessIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken: cancellationToken);

        // 格式化最终结果
        return topProcessUsage.Select(usage =>
            {
                var processInfo = processInfos[usage.ProcessId];
                var duration = usage.TotalDurationMilliseconds;

                return new ProcessUsageRankEntry(
                    processInfo.Id,
                    processInfo.Name,
                    processInfo.Alias,
                    processInfo.IconPath,
                    duration / 1000,
                    (int)(duration * 100 / totalMilliseconds)
                );
            });
    }
}
