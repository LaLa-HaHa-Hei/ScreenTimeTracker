namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageTimeline;

public record GetWebsiteUsageTimelineResponseItem(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);
