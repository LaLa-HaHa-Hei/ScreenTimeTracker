using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public record PatchUserSettingsRequest(
    OptionalValue<string> AppIconDirectory,
    OptionalValue<int> AppMetadataStaleThresholdMinutes,
    OptionalValue<int> ActiveAppUsageSessionAutoSaveIntervalSeconds,
    OptionalValue<bool> IsIdleDetectionEnabled,
    OptionalValue<int> IdleThresholdSeconds,
    OptionalValue<int> IdleDetectionPollingIntervalSeconds,
    OptionalValue<int> MinValidAppUsageSessionDurationSeconds,
    OptionalValue<int> AppUsageSessionMergeToleranceSeconds,
    OptionalValue<int> AppUsageSessionOptimizationIntervalMinutes,
    OptionalValue<int> DayCutoffHour
);
