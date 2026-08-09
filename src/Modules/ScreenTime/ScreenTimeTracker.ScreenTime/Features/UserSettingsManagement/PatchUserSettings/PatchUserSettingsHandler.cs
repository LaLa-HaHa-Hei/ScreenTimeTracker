using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public class PatchUserSettingsHandler(ScreenTimeDbContext context)
    : IRequestHandler<PatchUserSettingsCommand, ErrorOr<Updated>>
{
    public async ValueTask<ErrorOr<Updated>> Handle(
        PatchUserSettingsCommand request,
        CancellationToken cancellationToken
    )
    {
        UserSettings userSettings = await context.UserSettings.SingleAsync(cancellationToken);

        userSettings.Update(
            appTracking: request.AppTracking.HasValue
                ? new(MapAppTracking(request.AppTracking.Value))
                : default,
            websiteTracking: request.WebsiteTracking.HasValue
                ? new(MapWebsiteTracking(request.WebsiteTracking.Value))
                : default,
            idleDetection: request.IdleDetection.HasValue
                ? new(MapIdleDetection(request.IdleDetection.Value))
                : default,
            timeBoundary: request.TimeBoundary.HasValue
                ? new(MapTimeBoundary(request.TimeBoundary.Value))
                : default,
            regional: request.Regional.HasValue ? new(MapRegional(request.Regional.Value)) : default
        );

        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }

    private static AppTrackingSettings MapAppTracking(AppTrackingSettingsDto dto)
    {
        return new AppTrackingSettings(
            dto.IconDirectory,
            TimeSpan.FromMinutes(dto.MetadataStaleThresholdMinutes),
            TimeSpan.FromSeconds(dto.ActiveUsageSessionAutoSaveIntervalSeconds),
            TimeSpan.FromSeconds(dto.MinValidUsageSessionDurationSeconds),
            TimeSpan.FromSeconds(dto.UsageSessionMergeToleranceSeconds),
            TimeSpan.FromMinutes(dto.UsageSessionOptimizationIntervalMinutes)
        );
    }

    private static WebsiteTrackingSettings MapWebsiteTracking(WebsiteTrackingSettingsDto dto)
    {
        return new WebsiteTrackingSettings(
            dto.IconDirectory,
            TimeSpan.FromSeconds(dto.ActiveUsageSessionAutoSaveIntervalSeconds),
            TimeSpan.FromSeconds(dto.MinValidUsageSessionDurationSeconds),
            TimeSpan.FromSeconds(dto.UsageSessionMergeToleranceSeconds),
            TimeSpan.FromMinutes(dto.UsageSessionOptimizationIntervalMinutes)
        );
    }

    private static IdleDetectionSettings MapIdleDetection(IdleDetectionSettingsDto dto)
    {
        return new IdleDetectionSettings(
            dto.IsEnabled,
            TimeSpan.FromSeconds(dto.InactivityThresholdSeconds),
            TimeSpan.FromSeconds(dto.PollingIntervalSeconds)
        );
    }

    private static TimeBoundarySettings MapTimeBoundary(TimeBoundarySettingsDto dto)
    {
        return new TimeBoundarySettings(dto.DayCutoffHour);
    }

    private static RegionalSettings MapRegional(RegionalSettingsDto dto)
    {
        return new RegionalSettings(dto.TimeZoneId);
    }
}
