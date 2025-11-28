using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Commands.UpdateTrackerSettings;

public record UpdateTrackerSettingsCommand(
    TrackerSettingsDto Settings) : ICommand<Unit>
{
}
