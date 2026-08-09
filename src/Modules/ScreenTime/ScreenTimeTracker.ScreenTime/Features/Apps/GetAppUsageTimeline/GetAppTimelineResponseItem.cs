namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsageTimeline;

public record GetAppUsageTimelineResponseItem(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);
