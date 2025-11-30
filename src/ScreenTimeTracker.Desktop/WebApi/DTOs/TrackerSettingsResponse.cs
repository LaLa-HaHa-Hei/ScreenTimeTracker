namespace ScreenTimeTracker.Desktop.WebApi.DTOs;

public record TrackerSettingsResponse(
    int PollingIntervalMilliseconds,
    int ProcessInfoStaleThresholdHours,
    string ProcessIconDirPath,
    bool EnableIdleDetection,
    int IdleTimeoutSeconds
);
