using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.Application.DTOs;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries
{
    class UsageReportQueries(ScreenTimeDbContext dbContext) : IUsageReportQueries
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public async Task<IEnumerable<ProcessUsageRankEntry>> GetRankedProcessUsageForPeriodAsync(DateOnly startDate, DateOnly endDate, int topN = 10)
        {
            var start = startDate.ToDateTime(TimeOnly.MinValue);
            var end = endDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            // 将两个查询源定义为 IQueryable，并投影为统一的结构
            // 这一步不会执行数据库查询
            var summaryQuery = _dbContext.HourlySummaries
                .AsNoTracking()
                .Where(h => h.Hour >= start && h.Hour < end)
                .Select(h => new { ProcessId = h.ProcessInfoEntityId, Duration = (long)h.TotalDurationMilliseconds });
            var intervalQuery = _dbContext.ActivityIntervals
                .AsNoTracking()
                .Where(a => a.Timestamp >= start && a.Timestamp < end)
                .Select(a => new { ProcessId = a.ProcessInfoEntityId, Duration = (long)a.DurationMilliseconds });

            // 合并两个 IQueryable，然后进行分组和求和。
            // 查询尚未执行。
            var combinedUsageQuery = summaryQuery.Concat(intervalQuery)
                .GroupBy(u => u.ProcessId)
                .Select(g => new
                {
                    ProcessId = g.Key,
                    TotalDurationMilliseconds = g.Sum(u => u.Duration)
                });

            // 执行查询，一次性从数据库获取所有进程的总时长
            var allUsage = await combinedUsageQuery.ToListAsync();

            if (allUsage.Count == 0)
                return [];

            // 在内存中计算总时长
            long totalMilliseconds = allUsage.Sum(p => p.TotalDurationMilliseconds);
            if (totalMilliseconds == 0)
                return [];

            // 在内存中排序并获取 Top N
            var topProcessUsage = allUsage
                .OrderByDescending(p => p.TotalDurationMilliseconds)
                .Take(topN)
                .ToArray();

            var topProcessIds = topProcessUsage.Select(p => p.ProcessId).ToList();

            // 第二次数据库查询：获取 Top N 进程的详细信息
            var processInfos = await _dbContext.ProcessInfos
                .AsNoTracking()
                .Where(p => topProcessIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // 格式化最终结果
            return [.. topProcessUsage.Select(usage =>
            {
                var processInfo = processInfos[usage.ProcessId];
                var duration = usage.TotalDurationMilliseconds;
                var timeSpan = TimeSpan.FromMilliseconds(duration);
                var formattedDuration = $"{timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";

                return new ProcessUsageRankEntry(
                    processInfo.Id,
                    processInfo.Name,
                    processInfo.Alias,
                    processInfo.IconPath,
                    formattedDuration,
                    (int)(duration * 100 / totalMilliseconds)
                );
            })];
        }

        public async Task<IDictionary<int, TimeSpan>> GetTotalHourlyUsageForDayAsync(DateOnly date)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);

            var summaryQuery = _dbContext.HourlySummaries
                .AsNoTracking()
                .Where(h => h.Hour >= start && h.Hour < end)
                .Select(h => new { Hour = h.Hour.Hour, Milliseconds = (long)h.TotalDurationMilliseconds });

            var intervalQuery = _dbContext.ActivityIntervals
                .AsNoTracking()
                .Where(a => a.Timestamp >= start && a.Timestamp < end)
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
                h => TimeSpan.FromMilliseconds(h.TotalMilliseconds)
            );
        }

        public async Task<IDictionary<DateOnly, TimeSpan>> GetTotalDailyUsageForPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            var start = startDate.ToDateTime(TimeOnly.MinValue);
            var end = endDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            var summaryQuery = _dbContext.HourlySummaries
                .AsNoTracking()
                .Where(h => h.Hour >= start && h.Hour < end)
                .Select(h => new { Date = h.Hour.Date, Milliseconds = (long)h.TotalDurationMilliseconds });

            var intervalQuery = _dbContext.ActivityIntervals
                .AsNoTracking()
                .Where(a => a.Timestamp >= start && a.Timestamp < end)
                .Select(a => new { Date = a.Timestamp.Date, Milliseconds = (long)a.DurationMilliseconds });

            var dailyUsage = await summaryQuery.Concat(intervalQuery)
                .GroupBy(u => u.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalMilliseconds = g.Sum(u => u.Milliseconds)
                })
                .ToListAsync();

            return dailyUsage.ToDictionary(
                d => DateOnly.FromDateTime(d.Date),
                d => TimeSpan.FromMilliseconds(d.TotalMilliseconds)
            );
        }

        public async Task<IDictionary<DateOnly, TimeSpan>> GetProcessDailyDistributionForPeriodAsync(DateOnly startDate, DateOnly endDate, Guid processId)
        {
            var start = startDate.ToDateTime(TimeOnly.MinValue);
            var end = endDate.ToDateTime(TimeOnly.MinValue).AddDays(1);

            var summaryQuery = _dbContext.HourlySummaries
                .AsNoTracking()
                .Where(h => h.ProcessInfoEntityId == processId && h.Hour >= start && h.Hour < end)
                .Select(h => new { Date = h.Hour.Date, Milliseconds = (long)h.TotalDurationMilliseconds });

            var intervalQuery = _dbContext.ActivityIntervals
                .AsNoTracking()
                .Where(a => a.ProcessInfoEntityId == processId && a.Timestamp >= start && a.Timestamp < end)
                .Select(a => new { Date = a.Timestamp.Date, Milliseconds = (long)a.DurationMilliseconds });

            var dailyUsage = await summaryQuery.Concat(intervalQuery)
                .GroupBy(u => u.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalMilliseconds = g.Sum(u => u.Milliseconds)
                })
                .ToListAsync();

            return dailyUsage.ToDictionary(
                d => DateOnly.FromDateTime(d.Date),
                d => TimeSpan.FromMilliseconds(d.TotalMilliseconds)
            );
        }

        public async Task<IDictionary<int, TimeSpan>> GetProcessHourlyDistributionForDayAsync(DateOnly date, Guid processId)
        {
            var start = date.ToDateTime(TimeOnly.MinValue);
            var end = start.AddDays(1);

            var summaryQuery = _dbContext.HourlySummaries
                .AsNoTracking()
                .Where(h => h.ProcessInfoEntityId == processId && h.Hour >= start && h.Hour < end)
                .Select(h => new { Hour = h.Hour.Hour, Milliseconds = (long)h.TotalDurationMilliseconds });

            var intervalQuery = _dbContext.ActivityIntervals
                .AsNoTracking()
                .Where(a => a.ProcessInfoEntityId == processId && a.Timestamp >= start && a.Timestamp < end)
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
                h => TimeSpan.FromMilliseconds(h.TotalMilliseconds)
            );
        }
    }
}
