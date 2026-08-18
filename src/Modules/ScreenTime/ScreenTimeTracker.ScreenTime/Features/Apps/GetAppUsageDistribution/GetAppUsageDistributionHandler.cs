using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageDistribution;

public class GetAppUsageDistributionHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppUsageDistributionQuery, GetAppUsageDistributionResponse>
{
    public async ValueTask<GetAppUsageDistributionResponse> Handle(
        GetAppUsageDistributionQuery request,
        CancellationToken cancellationToken
    )
    {
        var settings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        var dayCutoffHour = settings.TimeBoundary.DayCutoffHour;
        var timeRange = LogicalDay.CalculateUtcWindow(
            request.StartDate,
            request.EndDate,
            settings.TimeBoundary.DayCutoffHour,
            request.TimeZoneInfo
        );

        var appQuery = context.Apps.AsNoTracking();

        if (request.IncludedIds is not null)
            appQuery = appQuery.Where(c => request.IncludedIds.Contains(c.Id));
        else if (request.ExcludedIds is not null)
            appQuery = appQuery.Where(c => !request.ExcludedIds.Contains(c.Id));

        var appMap = await appQuery
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Color,
                c.IconPath,
                c.IconPathLastUpdatedAt,
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (appMap.Count == 0)
            return new GetAppUsageDistributionResponse([], 0, 0, 0, 0);

        var appIds = appMap.Keys;

        var sessions = await context
            .AppUsageSessions.AsNoTracking()
            .Where(x =>
                appIds.Contains(x.AppId)
                && timeRange.Start < x.EndTime
                && x.StartTime < timeRange.End
            )
            .Select(x => new { x.AppId, x.UsagePeriod })
            .ToListAsync(cancellationToken);

        var durationByAppId = new Dictionary<Guid, TimeSpan>();

        foreach (var session in sessions)
        {
            TimeSpan overlap = timeRange.OverlapDuration(session.UsagePeriod);
            if (overlap == TimeSpan.Zero)
                continue;

            durationByAppId[session.AppId] =
                durationByAppId.GetValueOrDefault(session.AppId) + overlap;
        }

        int totalCount = durationByAppId.Count;

        long totalDurationSeconds = durationByAppId.Values.Sum(x => (long)x.TotalSeconds);

        var topNItems = durationByAppId
            .OrderByDescending(x => x.Value)
            .Take(request.TopN)
            .Select(kvp =>
            {
                var app = appMap[kvp.Key];
                return new AppUsageDistributionItem(
                    Id: kvp.Key,
                    Name: app.Name,
                    Color: app.Color,
                    IconPath: app.IconPath,
                    IconPathLastUpdatedAt: app.IconPathLastUpdatedAt,
                    DurationSeconds: (long)kvp.Value.TotalSeconds
                );
            })
            .ToList();

        int othersCount = totalCount - topNItems.Count;
        long topNDurationSeconds = topNItems.Sum(x => x.DurationSeconds);
        long othersDurationSeconds = Math.Max(0, totalDurationSeconds - topNDurationSeconds);

        return new GetAppUsageDistributionResponse(
            Items: topNItems,
            TotalCount: totalCount,
            TotalDurationSeconds: totalDurationSeconds,
            OthersCount: othersCount,
            OthersDurationSeconds: othersDurationSeconds
        );
    }
}
