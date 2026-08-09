using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.GetLocalSettings;

public record GetLocalSettingsResponse(
    UIOpenMode DefaultUIOpenMode,
    bool IsAutoStartEnabled,
    bool IsSilentStartEnabled,
    string Language
);
