using Mediator;
using ScreenTimeTracker.DesktopSettings.Contracts.IntegrationEvents;
using ScreenTimeTracker.DesktopSettings.Domain.Events;

namespace ScreenTimeTracker.DesktopSettings.Infrastructure.Integration.EventHandlers;

public class LocalSettingsUpdatedHandler(IPublisher publisher)
    : INotificationHandler<LocalSettingsUpdatedDomainEvent>
{
    public async ValueTask Handle(
        LocalSettingsUpdatedDomainEvent notification,
        CancellationToken cancellationToken
    )
    {
        var integrationEvent = new LocalSettingsUpdatedIntegrationEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            notification.DefaultUIOpenMode,
            notification.IsAutoStartEnabled,
            notification.IsSilentStartEnabled,
            notification.Language
        );

        await publisher.Publish(integrationEvent, cancellationToken);
    }
}
