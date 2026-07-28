using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.GetUserSettings;

public class GetUserSettingsEndpoint(IMediator mediator)
    : Endpoint<EmptyRequest, GetUserSettingsResponse>
{
    public override void Configure()
    {
        Get("user-settings");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        GetUserSettingsResult userSettings = await mediator.Send(new GetUserSettingsQuery(), ct);
        await Send.OkAsync(
            new GetUserSettingsResponse(
                AppIconDirectory: userSettings.AppIconDirectory,
                AppMetadataStaleThresholdMinutes: (int)
                    userSettings.AppMetadataStaleThreshold.TotalMinutes,
                ActiveAppUsageSessionAutoSaveIntervalSeconds: (int)
                    userSettings.ActiveAppUsageSessionAutoSaveInterval.TotalSeconds,
                IsIdleDetectionEnabled: userSettings.IsIdleDetectionEnabled,
                IdleThresholdSeconds: (int)userSettings.IdleThreshold.TotalSeconds,
                IdleDetectionPollingIntervalSeconds: (int)
                    userSettings.IdleDetectionPollingInterval.TotalSeconds,
                MinValidAppUsageSessionDurationSeconds: (int)
                    userSettings.MinValidAppUsageSessionDuration.TotalSeconds,
                AppUsageSessionMergeToleranceSeconds: (int)
                    userSettings.AppUsageSessionMergeTolerance.TotalSeconds,
                AppUsageSessionOptimizationIntervalMinutes: (int)
                    userSettings.AppUsageSessionOptimizationInterval.TotalMinutes,
                DayCutoffHour: userSettings.DayCutoffHour
            ),
            ct
        );
    }
}
