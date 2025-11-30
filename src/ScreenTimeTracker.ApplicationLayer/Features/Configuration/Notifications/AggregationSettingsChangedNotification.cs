using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Notifications;


public sealed record AggregationSettingsChangedNotification : INotification
{
    public required AggregationSettingsDto Settings { get; init; }
}