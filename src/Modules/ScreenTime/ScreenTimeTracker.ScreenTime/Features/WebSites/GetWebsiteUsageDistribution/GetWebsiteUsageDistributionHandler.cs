using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public class GetWebsiteUsageDistributionHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore
) : IRequestHandler<GetWebsiteUsageDistributionQuery, GetWebsiteUsageDistributionResponse>
{
    public async ValueTask<GetWebsiteUsageDistributionResponse> Handle(
        GetWebsiteUsageDistributionQuery request,
        CancellationToken cancellationToken
    )
    {
        var settings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(settings.Regional.TimeZoneId);
        var dayCutoffHour = settings.TimeBoundary.DayCutoffHour;
        var minTime = UsageTimeCalculator.GetLogicalDayStartInUtc(
            request.StartDate,
            dayCutoffHour,
            timeZoneInfo
        );
        var maxTime = UsageTimeCalculator.GetLogicalDayStartInUtc(
            request.EndDate.AddDays(1),
            dayCutoffHour,
            timeZoneInfo
        );
        var excludedIds = request.ExcludedIds?.ToHashSet() ?? [];

        var sessions = await context
            .WebsiteUsageSessions.Where(x =>
                !excludedIds.Contains(x.Website!.Id)
                && x.StartTime < maxTime
                && minTime <= x.EndTime
            )
            .Select(x => new
            {
                x.Website!.Id,
                x.Website.Name,
                x.Website.Color,
                x.Website.IconPath,
                x.Website.IconPathLastUpdatedAt,
                x.StartTime,
                x.EndTime,
            })
            .ToListAsync(cancellationToken);

        var activeSession = activeSessionStore.Current;
        if (activeSession is not null)
        {
            if (activeSession.StartTime < maxTime && minTime < activeSession.LastActiveAt)
            {
                var activeWebsite = await context
                    .Websites.AsNoTracking()
                    .SingleAsync(x => x.Id == activeSession.WebsiteId, cancellationToken);

                if (!excludedIds.Contains(activeWebsite.Id))
                    sessions.Add(
                        new
                        {
                            activeWebsite.Id,
                            activeWebsite.Name,
                            activeWebsite.Color,
                            activeWebsite.IconPath,
                            activeWebsite.IconPathLastUpdatedAt,
                            activeSession.StartTime,
                            EndTime = activeSession.LastActiveAt,
                        }
                    );
            }
        }

        var aggregatedUsage =
            new Dictionary<
                Guid,
                (
                    string Name,
                    string Color,
                    string? IconPath,
                    DateTimeOffset IconPathLastUpdatedAt,
                    long DurationMilliseconds
                )
            >();

        foreach (var session in sessions)
        {
            // 截取和请求时间段交集的时间 (避免多算范围外的时长)
            DateTimeOffset actualStart = minTime < session.StartTime ? session.StartTime : minTime;
            DateTimeOffset actualEnd = session.EndTime < maxTime ? session.EndTime : maxTime;

            if (actualStart < actualEnd)
            {
                long durationMilliseconds = (long)(actualEnd - actualStart).TotalMilliseconds;

                if (!aggregatedUsage.TryGetValue(session.Id, out var current))
                    aggregatedUsage[session.Id] = (
                        session.Name,
                        session.Color,
                        session.IconPath,
                        session.IconPathLastUpdatedAt,
                        durationMilliseconds
                    );
                else
                    aggregatedUsage[session.Id] = (
                        current.Name,
                        current.Color,
                        current.IconPath,
                        current.IconPathLastUpdatedAt,
                        current.DurationMilliseconds + durationMilliseconds
                    );
            }
        }

        // 计算总体宏观指标
        int totalCount = aggregatedUsage.Count;
        // 先把毫秒转为秒，防止 topN 是全部时加和少于总时长
        long totalDurationSeconds = aggregatedUsage.Values.Sum(x => x.DurationMilliseconds / 1000);

        // 排序截取Top N列表
        var topNItems = aggregatedUsage
            // 先按照使用时长倒序排列，再保留到秒，防止顺序不准确
            .OrderByDescending(x => x.Value.DurationMilliseconds)
            .Select(kvp => new WebsiteUsageDistributionItem(
                Id: kvp.Key,
                Name: kvp.Value.Name,
                Color: kvp.Value.Color,
                IconPath: kvp.Value.IconPath,
                IconPathLastUpdatedAt: kvp.Value.IconPathLastUpdatedAt,
                DurationSeconds: kvp.Value.DurationMilliseconds / 1000
            ))
            // 取前 TopN
            .Take(request.TopN)
            .ToList();

        // 倒推“其他”数据
        int othersCount = totalCount - topNItems.Count;
        long topNDurationSeconds = topNItems.Sum(x => x.DurationSeconds);
        long othersDurationSeconds = Math.Max(0, totalDurationSeconds - topNDurationSeconds);

        // 排序、格式化并取前 TopN 返回
        return new GetWebsiteUsageDistributionResponse(
            Items: topNItems,
            TotalCount: totalCount,
            TotalDurationSeconds: totalDurationSeconds,
            OthersCount: othersCount,
            OthersDurationSeconds: othersDurationSeconds
        );
    }
}
