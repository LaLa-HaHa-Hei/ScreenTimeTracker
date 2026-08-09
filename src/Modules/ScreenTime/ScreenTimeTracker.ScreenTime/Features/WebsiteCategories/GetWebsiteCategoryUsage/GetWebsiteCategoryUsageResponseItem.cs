namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsage;

public record GetWebsiteCategoryUsageResponseItem(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    long DurationSeconds
);
