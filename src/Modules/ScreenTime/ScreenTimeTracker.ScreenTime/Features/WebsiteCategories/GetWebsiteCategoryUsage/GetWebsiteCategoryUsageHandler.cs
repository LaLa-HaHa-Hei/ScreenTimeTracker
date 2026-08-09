using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsage;

public class GetWebsiteCategoryUsageHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : IRequestHandler<GetWebsiteCategoryUsageQuery, List<GetWebsiteCategoryUsageResponseItem>>
{
    public async ValueTask<List<GetWebsiteCategoryUsageResponseItem>> Handle(
        GetWebsiteCategoryUsageQuery request,
        CancellationToken cancellationToken
    )
    {
        var settings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(settings.Regional.TimeZoneId);
        var timeRanges = GenerateUtcTimeRanges(
            request.StartDate,
            request.EndDate,
            request.Granularity,
            timeZoneInfo,
            settings.TimeBoundary.DayCutoffHour
        );

        if (timeRanges.Count == 0)
            return [];

        var minTime = timeRanges.First().Start;
        var maxTime = timeRanges.Last().End;

        var query = context
            .WebsiteUsageSessions.AsNoTracking()
            .Where(x => x.StartTime < maxTime && minTime <= x.EndTime);

        if (request.IncludedIds is not null)
            query = query.Where(x => request.IncludedIds.Contains(x.Website!.CategoryId));
        else if (request.ExcludedIds is not null)
            query = query.Where(x => !request.ExcludedIds.Contains(x.Website!.CategoryId));

        var sessions = await query
            .Select(x => new { x.StartTime, x.EndTime })
            .ToListAsync(cancellationToken);

        var activeSession = activeSessionStore.Current;
        if (activeSession is not null)
        {
            var activeSessionEnd = timeProvider.GetUtcNow();

            if (activeSession.StartTime < maxTime && minTime < activeSessionEnd)
            {
                var activeWebsite = await context
                    .Websites.AsNoTracking()
                    .SingleAsync(x => x.Id == activeSession.WebsiteId, cancellationToken);
                var included = request.IncludedIds?.Contains(activeWebsite.CategoryId) ?? true;
                var notExcluded =
                    request.ExcludedIds is null
                    || !request.ExcludedIds.Contains(activeWebsite.CategoryId);
                var shouldInclude = request.IncludedIds is not null ? included : notExcluded;

                if (shouldInclude)
                    sessions.Add(
                        new { activeSession.StartTime, EndTime = timeProvider.GetUtcNow() }
                    );
            }
        }

        var durationMilliseconds = new long[timeRanges.Count];

        // 计算 UTC 区间交集
        foreach (var session in sessions)
        {
            // 裁剪 Session 时间在查询总范围内
            var sessionStart = session.StartTime < minTime ? minTime : session.StartTime;
            var sessionEnd = maxTime < session.EndTime ? maxTime : session.EndTime;

            if (sessionStart >= sessionEnd)
                continue;

            // 找到该 Session 影响的第一个桶索引
            int startIdx = FindFirstBucketIndex(timeRanges, sessionStart);

            // 只遍历可能重叠的桶
            for (int i = startIdx; i < timeRanges.Count; i++)
            {
                var bucket = timeRanges[i];

                // 如果桶的开始时间已经大于等于 Session 结束时间，后续桶不可能再重叠，直接 break
                if (bucket.Start >= sessionEnd)
                    break;

                // 计算重叠区间 [max(start), min(end)]
                var overlapStart = sessionStart > bucket.Start ? sessionStart : bucket.Start;
                var overlapEnd = sessionEnd < bucket.End ? sessionEnd : bucket.End;

                if (overlapStart < overlapEnd)
                {
                    durationMilliseconds[i] += (long)(overlapEnd - overlapStart).TotalMilliseconds;
                }
            }
        }

        // 组装最终结果（保留到秒）
        var result = new List<GetWebsiteCategoryUsageResponseItem>(timeRanges.Count);
        for (int i = 0; i < timeRanges.Count; i++)
        {
            result.Add(
                new GetWebsiteCategoryUsageResponseItem(
                    timeRanges[i].Start,
                    timeRanges[i].End,
                    durationMilliseconds[i] / 1000
                )
            );
        }

        return result;
    }

    /// <summary>
    /// 二分查找第一个 End > sessionStart 的桶索引
    /// </summary>
    private static int FindFirstBucketIndex(
        List<UtcTimeRange> timeRanges,
        DateTimeOffset sessionStart
    )
    {
        int low = 0,
            high = timeRanges.Count - 1;
        int result = timeRanges.Count;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (timeRanges[mid].End > sessionStart)
            {
                result = mid;
                high = mid - 1; // 尝试向前继续寻找更早重叠的桶
            }
            else
            {
                low = mid + 1;
            }
        }

        return result;
    }

    public readonly record struct UtcTimeRange
    {
        public DateTimeOffset Start { get; }
        public DateTimeOffset End { get; }

        public UtcTimeRange(DateTimeOffset start, DateTimeOffset end)
        {
            if (end < start)
            {
                throw new ArgumentException(
                    "End time must be greater than or equal to Start time."
                );
            }

            Start = start;
            End = end;
        }
    }

    private static List<UtcTimeRange> GenerateUtcTimeRanges(
        DateOnly startDate,
        DateOnly endDate,
        UsageGranularity granularity,
        TimeZoneInfo timeZoneInfo,
        int dayCutoffHour
    )
    {
        var timeRanges = new List<UtcTimeRange>();

        if (granularity == UsageGranularity.Day)
        {
            // 按逻辑天循环（闭区间 [startDate, endDate]）
            for (DateOnly d = startDate; d <= endDate; d = d.AddDays(1))
            {
                // 计算当前逻辑天的开始与结束时间（转换为 UTC）
                var dayStartUtc = UsageTimeCalculator.GetLogicalDayStartInUtc(
                    d,
                    dayCutoffHour,
                    timeZoneInfo
                );
                var dayEndUtc = UsageTimeCalculator.GetLogicalDayStartInUtc(
                    d.AddDays(1),
                    dayCutoffHour,
                    timeZoneInfo
                );

                timeRanges.Add(new(dayStartUtc, dayEndUtc));
            }
        }
        else if (granularity == UsageGranularity.Hour)
        {
            // 计算整体查询范围的起点和终点 UTC 时间
            var overallStartUtc = UsageTimeCalculator.GetLogicalDayStartInUtc(
                startDate,
                dayCutoffHour,
                timeZoneInfo
            );
            var overallEndUtc = UsageTimeCalculator.GetLogicalDayStartInUtc(
                endDate.AddDays(1),
                dayCutoffHour,
                timeZoneInfo
            );

            // 物理 1 小时递增生成桶
            var currentUtc = overallStartUtc;
            while (currentUtc < overallEndUtc)
            {
                var nextUtc = currentUtc.AddHours(1);
                timeRanges.Add(new(currentUtc, nextUtc));
                currentUtc = nextUtc;
            }
        }

        return timeRanges;
    }
}
