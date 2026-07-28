using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public record PatchUserSettingsCommand(
    OptionalValue<string> AppIconDirectory,
    OptionalValue<TimeSpan> AppMetadataStaleThreshold,
    OptionalValue<TimeSpan> ActiveAppUsageSessionAutoSaveInterval,
    OptionalValue<bool> IsIdleDetectionEnabled,
    OptionalValue<TimeSpan> IdleThreshold,
    OptionalValue<TimeSpan> IdleDetectionPollingInterval,
    OptionalValue<TimeSpan> MinValidAppUsageSessionDuration,
    OptionalValue<TimeSpan> AppUsageSessionMergeTolerance,
    OptionalValue<TimeSpan> AppUsageSessionOptimizationInterval,
    OptionalValue<int> DayCutoffHour
) : IRequest { }
