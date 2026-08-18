using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsage;

public class GetAppCategoryUsageHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppCategoryUsageQuery, List<GetAppCategoryUsageResponseItem>>
{
    public async ValueTask<List<GetAppCategoryUsageResponseItem>> Handle(
        GetAppCategoryUsageQuery request,
        CancellationToken cancellationToken
    )
    {
        var settings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        var timeRange = LogicalDay.CalculateUtcWindow(
            request.StartDate,
            request.EndDate,
            settings.TimeBoundary.DayCutoffHour,
            request.TimeZoneInfo
        );

        var timeBuckets = GenerateBuckets(
            request.StartDate,
            request.EndDate,
            request.Granularity,
            request.TimeZoneInfo,
            settings.TimeBoundary.DayCutoffHour
        );

        if (timeBuckets.Count == 0)
            return [];

        List<TimeRange> sessions;

        if (request.IncludedIds is not null)
            sessions = await context
                .AppUsageSessions.AsNoTracking()
                .Join(
                    context.Apps.AsNoTracking(),
                    session => session.AppId,
                    app => app.Id,
                    (session, app) => new { session, app }
                )
                .Where(x =>
                    request.IncludedIds.Contains(x.app.AppCategoryId)
                    && timeRange.Start < x.session.EndTime
                    && x.session.StartTime < timeRange.End
                )
                .Select(x => x.session.UsagePeriod)
                .ToListAsync(cancellationToken);
        else if (request.ExcludedIds is not null)
            sessions = await context
                .AppUsageSessions.AsNoTracking()
                .Join(
                    context.Apps.AsNoTracking(),
                    session => session.AppId,
                    app => app.Id,
                    (session, app) => new { session, app }
                )
                .Where(x =>
                    !request.ExcludedIds.Contains(x.app.AppCategoryId)
                    && timeRange.Start < x.session.EndTime
                    && x.session.StartTime < timeRange.End
                )
                .Select(x => x.session.UsagePeriod)
                .ToListAsync(cancellationToken);
        else
        {
            sessions = await context
                .AppUsageSessions.AsNoTracking()
                .Where(x => timeRange.Start < x.EndTime && x.StartTime < timeRange.End)
                .Select(x => x.UsagePeriod)
                .ToListAsync(cancellationToken);
        }

        var duration = new TimeSpan[timeBuckets.Count];

        foreach (var session in sessions)
        {
            TimeRange? validPart = session.Intersect(timeRange);

            if (validPart is null)
                continue;

            int startIdx = FindFirstBucketIndex(timeBuckets, validPart.Value.Start);

            for (int i = startIdx; i < timeBuckets.Count; i++)
            {
                var bucket = timeBuckets[i];
                if (bucket.Start >= validPart.Value.End)
                    break;

                duration[i] += bucket.OverlapDuration(validPart.Value);
            }
        }

        var result = new List<GetAppCategoryUsageResponseItem>(duration.Length);
        for (int i = 0; i < duration.Length; i++)
        {
            result.Add(
                new GetAppCategoryUsageResponseItem(
                    timeBuckets[i].Start,
                    timeBuckets[i].End,
                    (long)duration[i].TotalSeconds
                )
            );
        }

        return result;
    }

    private static List<TimeRange> GenerateBuckets(
        DateOnly startDate,
        DateOnly endDate,
        UsageGranularity granularity,
        TimeZoneInfo timeZoneInfo,
        int dayCutoffHour
    )
    {
        var timeRanges = new List<TimeRange>();

        if (granularity == UsageGranularity.Hour)
        {
            var startDay = LogicalDay.From(startDate, dayCutoffHour, timeZoneInfo);
            var endDay = LogicalDay.From(endDate, dayCutoffHour, timeZoneInfo);

            var overallStartUtc = startDay.UtcWindow.Start;
            var overallEndUtc = endDay.UtcWindow.End;

            var currentUtc = overallStartUtc;
            while (currentUtc < overallEndUtc)
            {
                var nextUtc = currentUtc.AddHours(1);
                timeRanges.Add(new TimeRange(currentUtc, nextUtc));
                currentUtc = nextUtc;
            }
        }
        else if (granularity == UsageGranularity.Day)
        {
            for (var d = startDate; d <= endDate; d = d.AddDays(1))
            {
                var logicalDay = LogicalDay.From(d, dayCutoffHour, timeZoneInfo);
                timeRanges.Add(logicalDay.UtcWindow);
            }
        }

        return timeRanges;
    }

    private static int FindFirstBucketIndex(
        List<TimeRange> timeBuckets,
        DateTimeOffset sessionStart
    )
    {
        int low = 0,
            high = timeBuckets.Count - 1;
        int result = timeBuckets.Count;

        while (low <= high)
        {
            int mid = low + (high - low) / 2;
            if (timeBuckets[mid].End > sessionStart)
            {
                result = mid;
                high = mid - 1;
            }
            else
            {
                low = mid + 1;
            }
        }

        return result;
    }
}
