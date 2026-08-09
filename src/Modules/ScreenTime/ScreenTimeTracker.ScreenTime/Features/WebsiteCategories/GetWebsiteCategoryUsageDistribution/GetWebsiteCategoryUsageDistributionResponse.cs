namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryUsageDistribution;

public record GetWebsiteCategoryUsageDistributionResponse(
    List<WebsiteCategoryUsageDistributionItem> Items,
    int TotalCount,
    long TotalDurationSeconds,
    int OthersCount,
    long OthersDurationSeconds
);

public record WebsiteCategoryUsageDistributionItem(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    long DurationSeconds
);
