using ErrorOr;
using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public record PatchLocalSettingsCommand(
    OptionalValue<UIOpenMode> DefaultUIOpenMode = default,
    OptionalValue<bool> IsAutoStartEnabled = default,
    OptionalValue<bool> IsSilentStartEnabled = default,
    OptionalValue<string> Language = default
) : IRequest<ErrorOr<Updated>>;
