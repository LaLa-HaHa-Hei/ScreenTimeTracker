using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public record PatchUserSettingsRequest(
    OptionalValue<AppTrackingSettingsDto> AppTracking = default,
    OptionalValue<WebsiteTrackingSettingsDto> WebsiteTracking = default,
    OptionalValue<IdleDetectionSettingsDto> IdleDetection = default,
    OptionalValue<TimeBoundarySettingsDto> TimeBoundary = default,
    OptionalValue<RegionalSettingsDto> Regional = default
);
