namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApps;

public record GetAppsResponseItem(
    Guid Id,
    string Name,
    string Color,
    string ProcessName,
    bool AllowMetadataAutoRefresh,
    DateTimeOffset MetadataLastRefreshedAt,
    Guid CategoryId,
    string? ExecutablePath,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
