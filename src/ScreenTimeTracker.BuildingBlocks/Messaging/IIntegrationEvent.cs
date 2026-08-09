using Mediator;

namespace ScreenTimeTracker.BuildingBlocks.Messaging;

public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
