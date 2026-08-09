using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Domain;

public record LocalSettingsUpdatedDomainEvent(
    UIOpenMode DefaultUIOpenMode,
    bool IsAutoStartEnabled,
    bool IsSilentStartEnabled,
    string Language
) : IDomainEvent;
