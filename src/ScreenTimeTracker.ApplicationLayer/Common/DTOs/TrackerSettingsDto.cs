namespace ScreenTimeTracker.ApplicationLayer.Common.DTOs;

public record TrackerSettingsDto(
    TimeSpan PollingInterval,
    TimeSpan ProcessInfoStaleThreshold,
    string ProcessIconDirPath,
    bool EnableIdleDetection,
    TimeSpan IdleTimeout
);
