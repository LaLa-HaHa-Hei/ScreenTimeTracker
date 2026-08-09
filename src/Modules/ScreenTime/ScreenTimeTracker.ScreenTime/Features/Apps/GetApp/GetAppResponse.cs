namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public record GetAppResponse(
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
