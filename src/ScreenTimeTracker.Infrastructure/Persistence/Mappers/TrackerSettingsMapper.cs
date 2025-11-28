using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Mappers;

internal class TrackerSettingsMapper
{
    public static TrackerSettings Map(TrackerSettingsEntity entity)
    {
        return new TrackerSettings(
            entity.PollingInterval,
            entity.ProcessInfoStaleThreshold,
            entity.ProcessIconDirPath,
            entity.EnableIdleDetection,
            entity.IdleTimeout
        );
    }

    public static TrackerSettingsEntity Map(TrackerSettings domain)
    {
        return new TrackerSettingsEntity
        {
            PollingInterval = domain.PollingInterval,
            ProcessInfoStaleThreshold = domain.ProcessInfoStaleThreshold,
            ProcessIconDirPath = domain.ProcessIconDirPath,
            EnableIdleDetection = domain.EnableIdleDetection,
            IdleTimeout = domain.IdleTimeout
        };
    }
}
