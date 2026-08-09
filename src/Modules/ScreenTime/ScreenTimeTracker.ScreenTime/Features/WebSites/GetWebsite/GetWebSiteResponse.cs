namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsite;

public record GetWebsiteResponse(
    Guid Id,
    string Name,
    string Color,
    string Host,
    bool AllowMetadataAutoRefresh,
    DateTimeOffset MetadataLastRefreshedAt,
    Guid CategoryId,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
