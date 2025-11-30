using Mediator;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateAggregationSettings;

public class UpdateAggregationSettingsCommandHandler(
        IUserConfigurationRepository repository,
        IMediator mediator)
    : ICommandHandler<UpdateAggregationSettingsCommand, Unit>
{
    private readonly IUserConfigurationRepository _userConfigRepository = repository;
    private readonly IMediator _mediator = mediator;

    public async ValueTask<Unit> Handle(UpdateAggregationSettingsCommand command, CancellationToken cancellationToken)
    {
        var userConfig = await _userConfigRepository.GetConfig();
        var settings = command.Settings;
        userConfig.Aggregation = new AggregationSettings(
            settings.PollingInterval
        );
        await _userConfigRepository.SaveConfig(userConfig);

        var notification = new AggregationSettingsChangedNotification
        {
            Settings = settings
        };

        await _mediator.Publish(notification, cancellationToken);

        return Unit.Value;
    }
}