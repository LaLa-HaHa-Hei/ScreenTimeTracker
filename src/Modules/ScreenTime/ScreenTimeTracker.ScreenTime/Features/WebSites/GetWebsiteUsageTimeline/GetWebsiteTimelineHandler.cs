using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageTimeline;

public class GetWebsiteUsageTimelineHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore
) : IRequestHandler<GetWebsiteUsageTimelineQuery, List<GetWebsiteUsageTimelineResponseItem>>
{
    public async ValueTask<List<GetWebsiteUsageTimelineResponseItem>> Handle(
        GetWebsiteUsageTimelineQuery request,
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

        var query = context
            .WebsiteUsageSessions.AsNoTracking()
            .Where(x => x.StartTime < maxTime && minTime <= x.EndTime);

        if (request.IncludedIds is not null)
            query = query.Where(x => request.IncludedIds.Contains(x.Website!.CategoryId));
        else if (request.ExcludedIds is not null)
            query = query.Where(x => !request.ExcludedIds.Contains(x.Website!.CategoryId));

        var sessions = await query
            .Select(x => new
            {
                x.Website!.Id,
                x.Website.Name,
                x.Website.Color,
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
                var included = request.IncludedIds?.Contains(activeWebsite.Id) ?? true;
                var notExcluded =
                    request.ExcludedIds is null || !request.ExcludedIds.Contains(activeWebsite.Id);
                var shouldInclude = request.IncludedIds is not null ? included : notExcluded;

                if (shouldInclude)
                    sessions.Add(
                        new
                        {
                            activeWebsite.Id,
                            activeWebsite.Name,
                            activeWebsite.Color,
                            activeSession.StartTime,
                            EndTime = activeSession.LastActiveAt,
                        }
                    );
            }
        }

        var normalized = sessions.Select(s =>
            s with
            {
                StartTime = s.StartTime < minTime ? minTime : s.StartTime,
                EndTime = maxTime < s.EndTime ? maxTime : s.EndTime,
            }
        );

        return
        [
            .. normalized
                .OrderBy(x => x.StartTime)
                .Select(x => new GetWebsiteUsageTimelineResponseItem(
                    Id: x.Id,
                    Name: x.Name,
                    Color: x.Color,
                    StartTime: x.StartTime,
                    EndTime: x.EndTime
                )),
        ];
    }
}
