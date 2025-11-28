using Mediator;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetTrackerSettings;

public record ResetTrackerSettingsCommand() : ICommand<Unit>
{
}
