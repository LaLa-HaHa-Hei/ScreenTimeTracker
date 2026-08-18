using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageTimeline;

public class GetAppCategoryUsageTimelineHandler(ScreenTimeDbContext context)
    : IRequestHandler<
        GetAppCategoryUsageTimelineQuery,
        List<GetAppCategoryUsageTimelineResponseItem>
    >
{
    public async ValueTask<List<GetAppCategoryUsageTimelineResponseItem>> Handle(
        GetAppCategoryUsageTimelineQuery request,
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

        var appCategoryQuery = context.AppCategories.AsNoTracking();

        if (request.IncludedIds is not null)
            appCategoryQuery = appCategoryQuery.Where(c => request.IncludedIds.Contains(c.Id));
        else if (request.ExcludedIds is not null)
            appCategoryQuery = appCategoryQuery.Where(c => !request.ExcludedIds.Contains(c.Id));

        var appCategoryMap = await appCategoryQuery
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Color,
            })
            .ToDictionaryAsync(c => c.Id, cancellationToken);

        if (appCategoryMap.Count == 0)
            return [];

        var appCategoryIds = appCategoryMap.Keys;

        var sessions = await context
            .AppUsageSessions.AsNoTracking()
            .Join(
                context.Apps.AsNoTracking(),
                session => session.AppId,
                app => app.Id,
                (session, app) =>
                    new
                    {
                        app.AppCategoryId,
                        session.UsagePeriod,
                        session.StartTime,
                        session.EndTime,
                    }
            )
            .Where(x =>
                appCategoryIds.Contains(x.AppCategoryId)
                && timeRange.Start < x.EndTime
                && x.StartTime < timeRange.End
            )
            .Select(x => new { x.AppCategoryId, x.UsagePeriod })
            .ToListAsync(cancellationToken);

        var result = new List<GetAppCategoryUsageTimelineResponseItem>(sessions.Count);

        foreach (var session in sessions)
        {
            var appCategory = appCategoryMap[session.AppCategoryId];
            var validPart = session.UsagePeriod.Intersect(timeRange);

            if (validPart is null)
                continue;

            result.Add(
                new GetAppCategoryUsageTimelineResponseItem(
                    Id: appCategory.Id,
                    Name: appCategory.Name,
                    Color: appCategory.Color,
                    StartTime: validPart.Value.Start,
                    EndTime: validPart.Value.End
                )
            );
        }

        result.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

        return result;
    }
}
