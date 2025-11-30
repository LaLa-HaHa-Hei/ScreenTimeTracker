using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;


public sealed record TrackerSettingsChangedNotification : INotification
{
    public required TrackerSettingsDto Settings { get; init; }
}