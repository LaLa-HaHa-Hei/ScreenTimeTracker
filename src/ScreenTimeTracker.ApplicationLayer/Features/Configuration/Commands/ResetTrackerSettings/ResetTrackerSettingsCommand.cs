using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.ResetTrackerSettings;

public record ResetTrackerSettingsCommand() : ICommand<Unit>
{
}
