using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.Desktop.WebApi.DTOs;

namespace ScreenTimeTracker.Desktop.WebApi.Mappers;

internal class TrackerSettingsMapper
{
    public static TrackerSettingsResponse Map(TrackerSettingsDto source)
    {
        return new TrackerSettingsResponse(
            (int)source.PollingInterval.TotalMilliseconds,
            (int)source.ProcessInfoStaleThreshold.TotalHours,
            source.ProcessIconDirPath,
            source.EnableIdleDetection,
            (int)source.IdleTimeout.TotalSeconds);
    }

    public static TrackerSettingsDto Map(UpdateTrackerSettingsRequest source)
    {
        return new TrackerSettingsDto(
            TimeSpan.FromMilliseconds(source.PollingIntervalMilliseconds),
            TimeSpan.FromHours(source.ProcessInfoStaleThresholdHours),
            source.ProcessIconDirPath,
            source.EnableIdleDetection,
            TimeSpan.FromSeconds(source.IdleTimeoutSeconds));
    }
}
