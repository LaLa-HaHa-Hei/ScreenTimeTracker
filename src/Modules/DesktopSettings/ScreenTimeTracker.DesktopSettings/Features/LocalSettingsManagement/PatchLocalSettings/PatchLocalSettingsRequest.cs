using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public record PatchLocalSettingsRequest(
    OptionalValue<UIOpenMode> DefaultUIOpenMode,
    OptionalValue<bool> IsAutoStartEnabled,
    OptionalValue<bool> IsSilentStartEnabled,
    OptionalValue<string> Language
);
