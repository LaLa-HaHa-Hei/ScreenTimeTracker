using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateTrackerSettings;

public record UpdateTrackerSettingsCommand(
    TrackerSettingsDto Settings) : ICommand<Unit>
{
}
