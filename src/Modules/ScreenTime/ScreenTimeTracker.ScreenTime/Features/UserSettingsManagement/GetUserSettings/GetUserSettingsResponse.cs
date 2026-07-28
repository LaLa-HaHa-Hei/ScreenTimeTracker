namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.GetUserSettings;

public record GetUserSettingsResponse(
    string AppIconDirectory,
    int AppMetadataStaleThresholdMinutes,
    int ActiveAppUsageSessionAutoSaveIntervalSeconds,
    bool IsIdleDetectionEnabled,
    int IdleThresholdSeconds,
    int IdleDetectionPollingIntervalSeconds,
    int MinValidAppUsageSessionDurationSeconds,
    int AppUsageSessionMergeToleranceSeconds,
    int AppUsageSessionOptimizationIntervalMinutes,
    int DayCutoffHour
);
