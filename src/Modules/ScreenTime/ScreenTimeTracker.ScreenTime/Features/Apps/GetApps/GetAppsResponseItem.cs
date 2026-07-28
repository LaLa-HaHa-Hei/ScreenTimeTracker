namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApps;

public record GetAppsResponseItem(
    Guid Id,
    string Name,
    string Color,
    string ProcessName,
    bool AllowMetadataAutoUpdate,
    DateTime MetadataLastUpdatedAt,
    Guid AppCategoryId,
    string? ExecutablePath,
    string? IconPath,
    DateTime IconPathLastUpdatedAt,
    bool IsSystem
);
