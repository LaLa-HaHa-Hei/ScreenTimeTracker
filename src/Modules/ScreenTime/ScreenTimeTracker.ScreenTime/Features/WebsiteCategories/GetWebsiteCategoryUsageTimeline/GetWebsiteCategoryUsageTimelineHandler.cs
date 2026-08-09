using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageTimeline;

public class GetWebsiteCategoryUsageTimelineHandler(
    ScreenTimeDbContext context,
    ActiveWebsiteUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
)
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
                Id = x.Website!.CategoryId,
                x.Website!.Category!.Name,
                x.Website!.Category!.Color,
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
                var activeWebsite = await context
                    .Websites.Include(a => a.Category)
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == activeSession.WebsiteId, cancellationToken);
                var included = request.IncludedIds?.Contains(activeWebsite.CategoryId) ?? true;
                var notExcluded =
                    request.ExcludedIds is null
                    || !request.ExcludedIds.Contains(activeWebsite.CategoryId);
                var shouldInclude = request.IncludedIds is not null ? included : notExcluded;

                if (shouldInclude)
                    sessions.Add(
                        new
                        {
                            Id = activeWebsite.CategoryId,
                            activeWebsite.Category!.Name,
                            activeWebsite.Category!.Color,
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
                .Select(x => new GetWebsiteCategoryUsageTimelineResponseItem(
                    Id: x.Id,
                    Name: x.Name,
                    Color: x.Color,
                    StartTime: x.StartTime,
                    EndTime: x.EndTime
                )),
        ];
    }
}
