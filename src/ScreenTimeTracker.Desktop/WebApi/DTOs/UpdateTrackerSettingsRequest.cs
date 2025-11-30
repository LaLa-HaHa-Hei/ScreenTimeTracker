namespace ScreenTimeTracker.Desktop.WebApi.DTOs;

public record UpdateTrackerSettingsRequest(
    int PollingIntervalMilliseconds,
    int ProcessInfoStaleThresholdHours,
    string ProcessIconDirPath,
    bool EnableIdleDetection,
    int IdleTimeoutSeconds
);
