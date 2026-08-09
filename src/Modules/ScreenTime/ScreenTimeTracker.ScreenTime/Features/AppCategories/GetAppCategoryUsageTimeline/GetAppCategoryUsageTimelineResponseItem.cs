namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageTimeline;

public record GetAppCategoryUsageTimelineResponseItem(
    Guid Id,
    string Name,
    string Color,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
);
