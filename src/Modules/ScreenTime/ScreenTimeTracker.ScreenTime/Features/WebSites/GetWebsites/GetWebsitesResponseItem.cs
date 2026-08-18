namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsites;

public record GetWebsitesResponseItem(
    Guid Id,
    string Name,
    string Color,
    string Host,
    bool AllowMetadataAutoRefresh,
    DateTimeOffset MetadataLastRefreshedAt,
    Guid WebsiteCategoryId,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
