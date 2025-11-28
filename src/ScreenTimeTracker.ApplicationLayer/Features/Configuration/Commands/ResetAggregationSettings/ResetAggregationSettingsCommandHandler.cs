using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.Mappers;
using ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateAggregationSettings;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetAggregationSettings;

public class ResetAggregationSettingsCommandHandler(
        IMediator mediator) : ICommandHandler<ResetAggregationSettingsCommand, Unit>
{
    private readonly IMediator _mediator = mediator;

    public ValueTask<Unit> Handle(ResetAggregationSettingsCommand command, CancellationToken cancellationToken)
    {
        _ = _mediator.Send(
            new UpdateAggregationSettingsCommand
            (
                Settings: AggregationSettingsMapper.Map(AggregationSettings.Default)
            )
        );
        return ValueTask.FromResult(Unit.Value);
    }
}