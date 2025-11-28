using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateAggregationSettings;

public record UpdateAggregationSettingsCommand(
    AggregationSettingsDto Settings) : ICommand<Unit>
{
}
