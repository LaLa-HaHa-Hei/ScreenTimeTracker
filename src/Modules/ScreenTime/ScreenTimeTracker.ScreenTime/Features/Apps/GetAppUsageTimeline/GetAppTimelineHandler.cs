using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageTimeline;

public class GetAppUsageTimelineHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppUsageTimelineQuery, List<GetAppUsageTimelineResponseItem>>
{
    public async ValueTask<List<GetAppUsageTimelineResponseItem>> Handle(
        GetAppUsageTimelineQuery request,
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
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (appMap.Count == 0)
            return [];

        var appIds = appMap.Keys;

        var sessions = await context
            .AppUsageSessions.AsNoTracking()
            .Where(x =>
                appIds.Contains(x.AppId)
                && timeRange.Start < x.EndTime
                && x.StartTime < timeRange.End
            )
            .ToListAsync(cancellationToken);

        var result = new List<GetAppUsageTimelineResponseItem>(sessions.Count);

        foreach (var session in sessions)
        {
            var app = appMap[session.AppId];
            var validPart = session.UsagePeriod.Intersect(timeRange);

            if (validPart is null)
                continue;

            result.Add(
                new GetAppUsageTimelineResponseItem(
                    Id: app.Id,
                    Name: app.Name,
                    Color: app.Color,
                    StartTime: validPart.Value.Start,
                    EndTime: validPart.Value.End
                )
            );
        }

        result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        return result;
    }
}
