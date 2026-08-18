using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageTimeline;

public class GetWebsiteCategoryUsageTimelineHandler(ScreenTimeDbContext context)
    : IRequestHandler<
        GetWebsiteCategoryUsageTimelineQuery,
        List<GetWebsiteCategoryUsageTimelineResponseItem>
    >
{
    public async ValueTask<List<GetWebsiteCategoryUsageTimelineResponseItem>> Handle(
        GetWebsiteCategoryUsageTimelineQuery request,
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
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (websiteCategoryMap.Count == 0)
            return [];

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

        var result = new List<GetWebsiteCategoryUsageTimelineResponseItem>(sessions.Count);

        foreach (var session in sessions)
        {
            var websiteCategory = websiteCategoryMap[session.WebsiteCategoryId];
            var validPart = session.UsagePeriod.Intersect(timeRange);

            if (validPart is null)
                continue;

            result.Add(
                new GetWebsiteCategoryUsageTimelineResponseItem(
                    Id: websiteCategory.Id,
                    Name: websiteCategory.Name,
                    Color: websiteCategory.Color,
                    StartTime: validPart.Value.Start,
                    EndTime: validPart.Value.End
                )
            );
        }

        result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        return result;
    }
}
