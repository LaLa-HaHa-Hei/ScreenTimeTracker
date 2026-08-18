using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public class GetWebsiteUsageDistributionHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsiteUsageDistributionQuery, GetWebsiteUsageDistributionResponse>
{
    public async ValueTask<GetWebsiteUsageDistributionResponse> Handle(
        GetWebsiteUsageDistributionQuery request,
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

        var websiteQuery = context.Websites.AsNoTracking();

        if (request.IncludedIds is not null)
            websiteQuery = websiteQuery.Where(c => request.IncludedIds.Contains(c.Id));
        else if (request.ExcludedIds is not null)
            websiteQuery = websiteQuery.Where(c => !request.ExcludedIds.Contains(c.Id));

        var websiteMap = await websiteQuery
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Color,
                c.IconPath,
                c.IconPathLastUpdatedAt,
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (websiteMap.Count == 0)
            return new GetWebsiteUsageDistributionResponse([], 0, 0, 0, 0);

        var websiteIds = websiteMap.Keys;

        var sessions = await context
            .WebsiteUsageSessions.AsNoTracking()
            .Where(x =>
                websiteIds.Contains(x.WebsiteId)
                && timeRange.Start < x.EndTime
                && x.StartTime < timeRange.End
            )
            .Select(x => new { x.WebsiteId, x.UsagePeriod })
            .ToListAsync(cancellationToken);

        var durationByWebsiteId = new Dictionary<Guid, TimeSpan>();

        foreach (var session in sessions)
        {
            TimeSpan overlap = timeRange.OverlapDuration(session.UsagePeriod);
            if (overlap == TimeSpan.Zero)
                continue;

            durationByWebsiteId[session.WebsiteId] =
                durationByWebsiteId.GetValueOrDefault(session.WebsiteId) + overlap;
        }

        int totalCount = durationByWebsiteId.Count;

        long totalDurationSeconds = durationByWebsiteId.Values.Sum(x => (long)x.TotalSeconds);

        var topNItems = durationByWebsiteId
            .OrderByDescending(x => x.Value)
            .Take(request.TopN)
            .Select(kvp =>
            {
                var website = websiteMap[kvp.Key];
                return new WebsiteUsageDistributionItem(
                    Id: kvp.Key,
                    Name: website.Name,
                    Color: website.Color,
                    IconPath: website.IconPath,
                    IconPathLastUpdatedAt: website.IconPathLastUpdatedAt,
                    DurationSeconds: (long)kvp.Value.TotalSeconds
                );
            })
            .ToList();

        int othersCount = totalCount - topNItems.Count;
        long topNDurationSeconds = topNItems.Sum(x => x.DurationSeconds);
        long othersDurationSeconds = Math.Max(0, totalDurationSeconds - topNDurationSeconds);

        return new GetWebsiteUsageDistributionResponse(
            Items: topNItems,
            TotalCount: totalCount,
            TotalDurationSeconds: totalDurationSeconds,
            OthersCount: othersCount,
            OthersDurationSeconds: othersDurationSeconds
        );
    }
}
