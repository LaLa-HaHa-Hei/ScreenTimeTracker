namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteUsageDistribution;

public record GetWebsiteUsageDistributionResponse(
    List<WebsiteUsageDistributionItem> Items,
    int TotalCount,
    long TotalDurationSeconds,
    int OthersCount,
    long OthersDurationSeconds
);

public record WebsiteUsageDistributionItem(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    long DurationSeconds
);
