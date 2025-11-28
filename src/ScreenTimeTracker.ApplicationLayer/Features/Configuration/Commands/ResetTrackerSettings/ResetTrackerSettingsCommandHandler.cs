using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.Mappers;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateTrackerSettings;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetTrackerSettings;

public class ResetTrackerSettingsCommandHandler(
        IMediator mediator) : ICommandHandler<ResetTrackerSettingsCommand, Unit>
{
    private readonly IMediator _mediator = mediator;

    public ValueTask<Unit> Handle(ResetTrackerSettingsCommand command, CancellationToken cancellationToken)
    {
        _ = _mediator.Send(
            new UpdateTrackerSettingsCommand
            (
                Settings: TrackerSettingsMapper.Map(TrackerSettings.Default)
            )
        );
        return ValueTask.FromResult(Unit.Value);
    }
}