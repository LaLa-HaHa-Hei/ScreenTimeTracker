using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public class PatchUserSettingsHandler(ScreenTimeDbContext context)
    : IRequestHandler<PatchUserSettingsCommand>
{
    public async ValueTask<Unit> Handle(
        PatchUserSettingsCommand request,
        CancellationToken cancellationToken
    )
    {
        UserSettings userSettings = await context.UserSettings.SingleAsync(cancellationToken);

        userSettings.Update(
            request.AppIconDirectory,
            request.AppMetadataStaleThreshold,
            request.ActiveAppUsageSessionAutoSaveInterval,
            request.IsIdleDetectionEnabled,
            request.IdleThreshold,
            request.IdleDetectionPollingInterval,
            request.MinValidAppUsageSessionDuration,
            request.AppUsageSessionMergeTolerance,
            request.AppUsageSessionOptimizationInterval,
            request.DayCutoffHour
        );

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
