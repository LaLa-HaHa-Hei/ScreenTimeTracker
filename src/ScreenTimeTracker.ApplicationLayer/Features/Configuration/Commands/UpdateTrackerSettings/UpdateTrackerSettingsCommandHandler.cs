using Mediator;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateTrackerSettings;

public class UpdateTrackerSettingsCommandHandler(
        IUserConfigurationRepository repository,
        IMediator mediator)
    : ICommandHandler<UpdateTrackerSettingsCommand, Unit>
{
    private readonly IUserConfigurationRepository _userConfigRepository = repository;
    private readonly IMediator _mediator = mediator;

    public async ValueTask<Unit> Handle(UpdateTrackerSettingsCommand command, CancellationToken cancellationToken)
    {
        var userConfig = await _userConfigRepository.GetConfig();
        var settings = command.Settings;
        userConfig.Tracker = new TrackerSettings(
            settings.PollingInterval,
            settings.ProcessInfoStaleThreshold,
            settings.ProcessIconDirPath,
            settings.EnableIdleDetection,
            settings.IdleTimeout
        );
        await _userConfigRepository.SaveConfig(userConfig);

        var notification = new TrackerSettingsChangedNotification
        {
            Settings = settings
        };

        await _mediator.Publish(notification, cancellationToken);

        return Unit.Value;
    }
}