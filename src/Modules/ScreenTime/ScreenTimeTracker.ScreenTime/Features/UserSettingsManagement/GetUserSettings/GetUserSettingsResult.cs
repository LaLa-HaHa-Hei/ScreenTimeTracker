namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.GetUserSettings;

public record GetUserSettingsResult(
    string AppIconDirectory,
    TimeSpan AppMetadataStaleThreshold,
    TimeSpan ActiveAppUsageSessionAutoSaveInterval,
    bool IsIdleDetectionEnabled,
    TimeSpan IdleThreshold,
    TimeSpan IdleDetectionPollingInterval,
    TimeSpan MinValidAppUsageSessionDuration,
    TimeSpan AppUsageSessionMergeTolerance,
    TimeSpan AppUsageSessionOptimizationInterval,
    int DayCutoffHour
);
