namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageTimeline;

public record GetWebsiteCategoryUsageTimelineResponseItem(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);
