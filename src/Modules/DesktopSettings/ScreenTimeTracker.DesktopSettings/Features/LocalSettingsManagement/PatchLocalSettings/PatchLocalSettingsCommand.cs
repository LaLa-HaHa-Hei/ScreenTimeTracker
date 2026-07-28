using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public record PatchLocalSettingsCommand(
    OptionalValue<UIOpenMode> DefaultUIOpenMode,
    OptionalValue<bool> IsAutoStartEnabled,
    OptionalValue<bool> IsSilentStartEnabled,
    OptionalValue<string> Language
) : IRequest;
