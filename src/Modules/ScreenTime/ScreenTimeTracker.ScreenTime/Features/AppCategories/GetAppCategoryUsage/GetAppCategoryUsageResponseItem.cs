namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsage;

public record GetAppCategoryUsageResponseItem(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    long DurationSeconds
);
