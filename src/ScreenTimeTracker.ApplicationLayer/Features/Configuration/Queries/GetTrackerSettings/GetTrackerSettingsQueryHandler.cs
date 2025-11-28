using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetTrackerSettings;

public class GetTrackerSettingsQueryHandler(IUserConfigurationRepository userConfigRepository) : IQueryHandler<GetTrackerSettingsQuery, TrackerSettingsDto>
{
    private readonly IUserConfigurationRepository _userConfigRepository = userConfigRepository;

    public async ValueTask<TrackerSettingsDto> Handle(GetTrackerSettingsQuery query, CancellationToken cancellationToken)
    {
        UserConfiguration userConfig = await _userConfigRepository.GetConfig();
        TrackerSettings setting = userConfig.Tracker;
        return new TrackerSettingsDto(
            PollingInterval: setting.PollingInterval,
            ProcessInfoStaleThreshold: setting.ProcessInfoStaleThreshold,
            ProcessIconDirPath: setting.ProcessIconDirPath,
            EnableIdleDetection: setting.EnableIdleDetection,
            IdleTimeout: setting.IdleTimeout
        );
    }
}