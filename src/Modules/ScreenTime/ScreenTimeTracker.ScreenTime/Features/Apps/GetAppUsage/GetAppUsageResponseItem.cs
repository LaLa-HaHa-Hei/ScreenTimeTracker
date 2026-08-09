namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppUsage;

public record GetAppUsageResponseItem(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    long DurationSeconds
);
