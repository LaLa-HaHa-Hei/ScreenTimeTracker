using Mediator;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetAggregationSettings;

public record ResetAggregationSettingsCommand() : ICommand<Unit>
{
}
