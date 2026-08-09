using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageTimeline;

public class GetAppUsageTimelineHandler(
    ScreenTimeDbContext context,
    ActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : IRequestHandler<GetAppUsageTimelineQuery, List<GetAppUsageTimelineResponseItem>>
{
    public async ValueTask<List<GetAppUsageTimelineResponseItem>> Handle(
        GetAppUsageTimelineQuery request,
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
            .AppUsageSessions.AsNoTracking()
            .Where(x => x.StartTime < maxTime && minTime <= x.EndTime);

        if (request.IncludedIds is not null)
            query = query.Where(x => request.IncludedIds.Contains(x.App!.CategoryId));
        else if (request.ExcludedIds is not null)
            query = query.Where(x => !request.ExcludedIds.Contains(x.App!.CategoryId));

        var sessions = await query
            .Select(x => new
            {
                x.App!.Id,
                x.App.Name,
                x.App.Color,
                x.StartTime,
                x.EndTime,
            })
            .ToListAsync(cancellationToken);

        var activeSession = activeSessionStore.Current;
        if (activeSession is not null)
        {
            var activeSessionEnd = timeProvider.GetUtcNow();

            if (activeSession.StartTime < maxTime && minTime < activeSessionEnd)
            {
                var activeApp = await context
                    .Apps.AsNoTracking()
                    .SingleAsync(x => x.Id == activeSession.AppId, cancellationToken);
                var included = request.IncludedIds?.Contains(activeApp.Id) ?? true;
                var notExcluded =
                    request.ExcludedIds is null || !request.ExcludedIds.Contains(activeApp.Id);
                var shouldInclude = request.IncludedIds is not null ? included : notExcluded;

                if (shouldInclude)
                    sessions.Add(
                        new
                        {
                            activeApp.Id,
                            activeApp.Name,
                            activeApp.Color,
                            activeSession.StartTime,
                            EndTime = activeSessionEnd,
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
                .Select(x => new GetAppUsageTimelineResponseItem(
                    Id: x.Id,
                    Name: x.Name,
                    Color: x.Color,
                    StartTime: x.StartTime,
                    EndTime: x.EndTime
                )),
        ];
    }
}
