using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public record PatchLocalSettingsRequest(
    OptionalValue<UIOpenMode> DefaultUIOpenMode = default,
    OptionalValue<bool> IsAutoStartEnabled = default,
    OptionalValue<bool> IsSilentStartEnabled = default,
    OptionalValue<string> Language = default
);
