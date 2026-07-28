using FastEndpoints;
using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public class PatchUserSettingsEndpoint(IMediator mediator)
    : Endpoint<PatchUserSettingsRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("user-settings");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchUserSettingsRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new PatchUserSettingsCommand(
                req.AppIconDirectory,
                req.AppMetadataStaleThresholdMinutes.HasValue
                    ? new(TimeSpan.FromMinutes(req.AppMetadataStaleThresholdMinutes.Value))
                    : default,
                req.ActiveAppUsageSessionAutoSaveIntervalSeconds.HasValue
                    ? new(
                        TimeSpan.FromSeconds(req.ActiveAppUsageSessionAutoSaveIntervalSeconds.Value)
                    )
                    : default,
                req.IsIdleDetectionEnabled,
                req.IdleThresholdSeconds.HasValue
                    ? new(TimeSpan.FromSeconds(req.IdleThresholdSeconds.Value))
                    : default,
                req.IdleDetectionPollingIntervalSeconds.HasValue
                    ? new(TimeSpan.FromSeconds(req.IdleDetectionPollingIntervalSeconds.Value))
                    : default,
                req.MinValidAppUsageSessionDurationSeconds.HasValue
                    ? new(TimeSpan.FromSeconds(req.MinValidAppUsageSessionDurationSeconds.Value))
                    : default,
                req.AppUsageSessionMergeToleranceSeconds.HasValue
                    ? new(TimeSpan.FromSeconds(req.AppUsageSessionMergeToleranceSeconds.Value))
                    : default,
                req.AppUsageSessionOptimizationIntervalMinutes.HasValue
                    ? new(
                        TimeSpan.FromMinutes(req.AppUsageSessionOptimizationIntervalMinutes.Value)
                    )
                    : default,
                req.DayCutoffHour
            ),
            ct
        );
        await Send.NoContentAsync(ct);
    }
}
