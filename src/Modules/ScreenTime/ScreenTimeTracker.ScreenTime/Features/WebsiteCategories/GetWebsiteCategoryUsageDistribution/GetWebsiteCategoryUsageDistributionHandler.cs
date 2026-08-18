using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageDistribution;

public class GetWebsiteCategoryUsageDistributionHandler(ScreenTimeDbContext context)
    : IRequestHandler<
        GetWebsiteCategoryUsageDistributionQuery,
        GetWebsiteCategoryUsageDistributionResponse
    >
{
    public async ValueTask<GetWebsiteCategoryUsageDistributionResponse> Handle(
        GetWebsiteCategoryUsageDistributionQuery request,
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

        var websiteCategoryQuery = context.WebsiteCategories.AsNoTracking();

        if (request.IncludedIds is not null)
            websiteCategoryQuery = websiteCategoryQuery.Where(c =>
                request.IncludedIds.Contains(c.Id)
            );
        else if (request.ExcludedIds is not null)
            websiteCategoryQuery = websiteCategoryQuery.Where(c =>
                !request.ExcludedIds.Contains(c.Id)
            );

        var websiteCategoryMap = await websiteCategoryQuery
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Color,
                c.IconPath,
                c.IconPathLastUpdatedAt,
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (websiteCategoryMap.Count == 0)
            return new GetWebsiteCategoryUsageDistributionResponse([], 0, 0, 0, 0);

        var websiteCategoryIds = websiteCategoryMap.Keys;

        var sessions = await context
            .WebsiteUsageSessions.AsNoTracking()
            .Join(
                context.Websites.AsNoTracking(),
                session => session.WebsiteId,
                website => website.Id,
                (session, website) =>
                    new
                    {
                        website.WebsiteCategoryId,
                        session.UsagePeriod,
                        session.StartTime,
                        session.EndTime,
                    }
            )
            .Where(x =>
                websiteCategoryIds.Contains(x.WebsiteCategoryId)
                && timeRange.Start < x.EndTime
                && x.StartTime < timeRange.End
            )
            .Select(x => new { x.WebsiteCategoryId, x.UsagePeriod })
            .ToListAsync(cancellationToken);

        var durationByWebsiteCategoryId = new Dictionary<Guid, TimeSpan>();

        foreach (var session in sessions)
        {
            TimeSpan overlap = timeRange.OverlapDuration(session.UsagePeriod);
            if (overlap == TimeSpan.Zero)
                continue;

            durationByWebsiteCategoryId[session.WebsiteCategoryId] =
                durationByWebsiteCategoryId.GetValueOrDefault(session.WebsiteCategoryId) + overlap;
        }

        int totalCount = durationByWebsiteCategoryId.Count;

        long totalDurationSeconds = durationByWebsiteCategoryId.Values.Sum(x =>
            (long)x.TotalSeconds
        );

        var topNItems = durationByWebsiteCategoryId
            .OrderByDescending(x => x.Value)
            .Take(request.TopN)
            .Select(kvp =>
            {
                var category = websiteCategoryMap[kvp.Key];
                return new WebsiteCategoryUsageDistributionItem(
                    Id: kvp.Key,
                    Name: category.Name,
                    Color: category.Color,
                    IconPath: category.IconPath,
                    IconPathLastUpdatedAt: category.IconPathLastUpdatedAt,
                    DurationSeconds: (long)kvp.Value.TotalSeconds
                );
            })
            .ToList();

        int othersCount = totalCount - topNItems.Count;
        long topNDurationSeconds = topNItems.Sum(x => x.DurationSeconds);
        long othersDurationSeconds = Math.Max(0, totalDurationSeconds - topNDurationSeconds);

        return new GetWebsiteCategoryUsageDistributionResponse(
            Items: topNItems,
            TotalCount: totalCount,
            TotalDurationSeconds: totalDurationSeconds,
            OthersCount: othersCount,
            OthersDurationSeconds: othersDurationSeconds
        );
    }
}
