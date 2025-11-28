using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Common.Mappers;

internal class TrackerSettingsMapper
{
    public static TrackerSettings Map(TrackerSettingsDto entity)
    {
        return new TrackerSettings(
            entity.PollingInterval,
            entity.ProcessInfoStaleThreshold,
            entity.ProcessIconDirPath,
            entity.EnableIdleDetection,
            entity.IdleTimeout
        );
    }

    public static TrackerSettingsDto Map(TrackerSettings domain)
    {
        return new TrackerSettingsDto
        (
            PollingInterval: domain.PollingInterval,
            ProcessInfoStaleThreshold: domain.ProcessInfoStaleThreshold,
            ProcessIconDirPath: domain.ProcessIconDirPath,
            EnableIdleDetection: domain.EnableIdleDetection,
            IdleTimeout: domain.IdleTimeout
        );
    }
}

