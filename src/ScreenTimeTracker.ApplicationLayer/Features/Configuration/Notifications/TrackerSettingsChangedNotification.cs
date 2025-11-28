using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;


#pragma warning disable MSG0005 // MediatorGenerator message warning
public sealed record TrackerSettingsChangedNotification : INotification
#pragma warning restore MSG0005 // MediatorGenerator message warning
{
    public required TrackerSettingsDto Settings { get; init; }
}