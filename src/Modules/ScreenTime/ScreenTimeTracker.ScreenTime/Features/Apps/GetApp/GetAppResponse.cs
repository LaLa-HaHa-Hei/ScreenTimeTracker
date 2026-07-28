namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public record GetAppResponse(
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
