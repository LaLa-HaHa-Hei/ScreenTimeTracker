namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsage;

public record GetWebsiteUsageResponseItem(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    long DurationSeconds
);
