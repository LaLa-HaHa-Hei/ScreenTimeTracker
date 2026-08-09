using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageTimeline;

public class GetAppCategoryUsageTimelineHandler(
    ScreenTimeDbContext context,
    ActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : IRequestHandler<GetAppCategoryUsageTimelineQuery, List<GetAppCategoryUsageTimelineResponseItem>>
{
    public async ValueTask<List<GetAppCategoryUsageTimelineResponseItem>> Handle(
        GetAppCategoryUsageTimelineQuery request,
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
                Id = x.App!.CategoryId,
                x.App!.Category!.Name,
                x.App!.Category!.Color,
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
                    .Apps.Include(a => a.Category)
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == activeSession.AppId, cancellationToken);
                var included = request.IncludedIds?.Contains(activeApp.CategoryId) ?? true;
                var notExcluded =
                    request.ExcludedIds is null
                    || !request.ExcludedIds.Contains(activeApp.CategoryId);
                var shouldInclude = request.IncludedIds is not null ? included : notExcluded;

                if (shouldInclude)
                    sessions.Add(
                        new
                        {
                            Id = activeApp.CategoryId,
                            activeApp.Category!.Name,
                            activeApp.Category!.Color,
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
                .Select(x => new GetAppCategoryUsageTimelineResponseItem(
                    Id: x.Id,
                    Name: x.Name,
                    Color: x.Color,
                    StartTime: x.StartTime,
                    EndTime: x.EndTime
                )),
        ];
    }
}
