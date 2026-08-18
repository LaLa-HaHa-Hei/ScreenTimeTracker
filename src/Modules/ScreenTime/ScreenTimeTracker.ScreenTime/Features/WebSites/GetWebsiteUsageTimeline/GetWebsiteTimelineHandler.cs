using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageTimeline;

public class GetWebsiteUsageTimelineHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsiteUsageTimelineQuery, List<GetWebsiteUsageTimelineResponseItem>>
{
    public async ValueTask<List<GetWebsiteUsageTimelineResponseItem>> Handle(
        GetWebsiteUsageTimelineQuery request,
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
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (websiteMap.Count == 0)
            return [];

        var websiteIds = websiteMap.Keys;

        var sessions = await context
            .WebsiteUsageSessions.AsNoTracking()
            .Where(x =>
                websiteIds.Contains(x.WebsiteId)
                && timeRange.Start < x.EndTime
                && x.StartTime < timeRange.End
            )
            .ToListAsync(cancellationToken);

        var result = new List<GetWebsiteUsageTimelineResponseItem>(sessions.Count);

        foreach (var session in sessions)
        {
            var website = websiteMap[session.WebsiteId];
            var validPart = session.UsagePeriod.Intersect(timeRange);

            if (validPart is null)
                continue;

            result.Add(
                new GetWebsiteUsageTimelineResponseItem(
                    Id: website.Id,
                    Name: website.Name,
                    Color: website.Color,
                    StartTime: validPart.Value.Start,
                    EndTime: validPart.Value.End
                )
            );
        }

        result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        return result;
    }
}
