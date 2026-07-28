using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.GetUserSettings;

public class GetUserSettingsHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetUserSettingsQuery, GetUserSettingsResult>
{
    public async ValueTask<GetUserSettingsResult> Handle(
        GetUserSettingsQuery request,
        CancellationToken cancellationToken
    )
    {
        UserSettings userSettings = await context
            .UserSettings.AsNoTracking()
            .SingleAsync(cancellationToken);

        return new GetUserSettingsResult(
            AppIconDirectory: userSettings.AppIconDirectory,
            AppMetadataStaleThreshold: userSettings.AppMetadataStaleThreshold,
            ActiveAppUsageSessionAutoSaveInterval: userSettings.ActiveAppUsageSessionAutoSaveInterval,
            IsIdleDetectionEnabled: userSettings.IsIdleDetectionEnabled,
            IdleThreshold: userSettings.IdleThreshold,
            IdleDetectionPollingInterval: userSettings.IdleDetectionPollingInterval,
            MinValidAppUsageSessionDuration: userSettings.MinValidAppUsageSessionDuration,
            AppUsageSessionMergeTolerance: userSettings.AppUsageSessionMergeTolerance,
            AppUsageSessionOptimizationInterval: userSettings.AppUsageSessionOptimizationInterval,
            DayCutoffHour: userSettings.DayCutoffHour
        );
    }
}
