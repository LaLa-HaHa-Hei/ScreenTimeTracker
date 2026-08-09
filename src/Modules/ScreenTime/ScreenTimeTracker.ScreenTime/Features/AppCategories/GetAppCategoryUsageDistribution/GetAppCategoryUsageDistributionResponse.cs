namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryUsageDistribution;

public record GetAppCategoryUsageDistributionResponse(
    List<AppCategoryUsageDistributionItem> Items,
    int TotalCount,
    long TotalDurationSeconds,
    int OthersCount,
    long OthersDurationSeconds
);

public record AppCategoryUsageDistributionItem(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    long DurationSeconds
);
