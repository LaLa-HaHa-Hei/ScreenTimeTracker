using ScreenTimeTracker.BuildingBlocks.Messaging;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Contracts.IntegrationEvents;

public record LocalSettingsUpdatedIntegrationEvent(
    Guid EventId,
    DateTime OccurredOn,
    UIOpenMode DefaultUIOpenMode,
    bool IsAutoStartEnabled,
    bool IsSilentStartEnabled,
    string Language
) : IIntegrationEvent;
