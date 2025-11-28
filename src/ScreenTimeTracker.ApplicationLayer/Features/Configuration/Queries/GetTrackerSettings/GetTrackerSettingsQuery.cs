using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetTrackerSettings;

public record GetTrackerSettingsQuery : IQuery<TrackerSettingsDto>
{
}