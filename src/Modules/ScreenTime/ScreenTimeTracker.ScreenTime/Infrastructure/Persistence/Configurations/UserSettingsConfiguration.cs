using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            // 不支持复杂属性的种子数据
            // builder.HasData(UserSettings.CreateDefault());

            builder.ComplexProperty(
                s => s.AppTracking,
                appTracking =>
                {
                    appTracking
                        .Property(nameof(AppTrackingSettings.IconDirectory))
                        .HasColumnName(
                            $"{nameof(UserSettings.AppTracking)}_{nameof(AppTrackingSettings.IconDirectory)}"
                        );
                    appTracking
                        .Property(p => p.MetadataStaleThreshold)
                        .HasColumnName(
                            $"{nameof(UserSettings.AppTracking)}_{nameof(AppTrackingSettings.MetadataStaleThreshold)}"
                        );
                    appTracking
                        .Property(p => p.ActiveUsageSessionAutoSaveInterval)
                        .HasColumnName(
                            $"{nameof(UserSettings.AppTracking)}_{nameof(AppTrackingSettings.ActiveUsageSessionAutoSaveInterval)}"
                        );
                    appTracking
                        .Property(p => p.MinValidUsageSessionDuration)
                        .HasColumnName(
                            $"{nameof(UserSettings.AppTracking)}_{nameof(AppTrackingSettings.MinValidUsageSessionDuration)}"
                        );
                    appTracking
                        .Property(p => p.UsageSessionMergeTolerance)
                        .HasColumnName(
                            $"{nameof(UserSettings.AppTracking)}_{nameof(AppTrackingSettings.UsageSessionMergeTolerance)}"
                        );
                    appTracking
                        .Property(p => p.UsageSessionOptimizationInterval)
                        .HasColumnName(
                            $"{nameof(UserSettings.AppTracking)}_{nameof(AppTrackingSettings.UsageSessionOptimizationInterval)}"
                        );
                }
            );

            builder.ComplexProperty(
                s => s.WebsiteTracking,
                websiteTracking =>
                {
                    websiteTracking
                        .Property(nameof(WebsiteTrackingSettings.IconDirectory))
                        .HasColumnName(
                            $"{nameof(UserSettings.WebsiteTracking)}_{nameof(WebsiteTrackingSettings.IconDirectory)}"
                        );
                    websiteTracking
                        .Property(p => p.ActiveUsageSessionAutoSaveInterval)
                        .HasColumnName(
                            $"{nameof(UserSettings.WebsiteTracking)}_{nameof(WebsiteTrackingSettings.ActiveUsageSessionAutoSaveInterval)}"
                        );
                    websiteTracking
                        .Property(p => p.MinValidUsageSessionDuration)
                        .HasColumnName(
                            $"{nameof(UserSettings.WebsiteTracking)}_{nameof(WebsiteTrackingSettings.MinValidUsageSessionDuration)}"
                        );
                    websiteTracking
                        .Property(p => p.UsageSessionMergeTolerance)
                        .HasColumnName(
                            $"{nameof(UserSettings.WebsiteTracking)}_{nameof(WebsiteTrackingSettings.UsageSessionMergeTolerance)}"
                        );
                    websiteTracking
                        .Property(p => p.UsageSessionOptimizationInterval)
                        .HasColumnName(
                            $"{nameof(UserSettings.WebsiteTracking)}_{nameof(WebsiteTrackingSettings.UsageSessionOptimizationInterval)}"
                        );
                }
            );

            builder.ComplexProperty(
                s => s.IdleDetection,
                idleDetection =>
                {
                    idleDetection
                        .Property(p => p.IsEnabled)
                        .HasColumnName(
                            $"{nameof(UserSettings.IdleDetection)}_{nameof(IdleDetectionSettings.IsEnabled)}"
                        );
                    idleDetection
                        .Property(p => p.InactivityThreshold)
                        .HasColumnName(
                            $"{nameof(UserSettings.IdleDetection)}_{nameof(IdleDetectionSettings.InactivityThreshold)}"
                        );
                    idleDetection
                        .Property(p => p.PollingInterval)
                        .HasColumnName(
                            $"{nameof(UserSettings.IdleDetection)}_{nameof(IdleDetectionSettings.PollingInterval)}"
                        );
                }
            );

            builder.ComplexProperty(
                s => s.TimeBoundary,
                timeBoundary =>
                {
                    timeBoundary
                        .Property(p => p.DayCutoffHour)
                        .HasColumnName(
                            $"{nameof(UserSettings.TimeBoundary)}_{nameof(TimeBoundarySettings.DayCutoffHour)}"
                        );
                }
            );

            builder.ComplexProperty(
                s => s.Regional,
                regional =>
                {
                    regional
                        .Property(p => p.TimeZoneId)
                        .HasColumnName(
                            $"{nameof(UserSettings.Regional)}_{nameof(RegionalSettings.TimeZoneId)}"
                        );
                }
            );
        }
    }
}
