using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.BuildingBlocks.Persistence;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Features.Tracking;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;
using ScreenTimeTracker.ScreenTime.Infrastructure.OS;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScreenTimeServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // 数据库
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddDbContext<ScreenTimeDbContext>(
            (serviceProvider, options) =>
            {
                var databaseOptions = serviceProvider
                    .GetRequiredService<IOptions<DatabaseOptions>>()
                    .Value;

                options.UseSqlite(
                    $"Data Source={databaseOptions.DBFilePath}",
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory_ScreenTime")
                );
            }
        );
        services.AddHostedService<ScreenTimeDbMigrationService>();

        // 处理器服务
        services.AddSingleton<ForegroundWindowProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<ForegroundWindowProcessor>());
        services.AddSingleton<WebsiteActivityProcessor>();
        services.AddHostedService(sp => sp.GetRequiredService<WebsiteActivityProcessor>());

        // 后台服务
        services.AddHostedService<AppUsageActiveSessionTracker>();
        services.AddHostedService<ActiveAppUsageSessionAutoSaver>();
        services.AddHostedService<ActiveWebsiteUsageSessionAutoSaver>();
        services.AddHostedService<AppUsageSessionOptimizationWorker>();
        services.AddHostedService<WebsiteUsageSessionOptimizationWorker>();
        services.AddHostedService<SystemSuspendMonitor>();
        services.AddHostedService<UserIdleMonitor>();
        services.AddHostedService<TimerDriftMonitor>();

        // 状态存储
        services.AddSingleton<ActiveAppUsageSessionStore>();
        services.AddSingleton<ActiveWebsiteUsageSessionStore>();
        services.AddSingleton<SystemSuspendStore>();
        services.AddSingleton<UserIdleStore>();

        // 平台特异服务
        if (OperatingSystem.IsWindows())
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(6, 0, 6000))
            {
                services.AddSingleton<IForegroundWindowMonitor, WindowsForegroundWindowMonitor>();
                services.AddSingleton<
                    IExecutableMetadataProvider,
                    WindowsExecutableMetadataProvider
                >();
                services.AddSingleton<IIdleTimeProvider, WindowsIdleTimeProvider>();
                services.AddSingleton<ISystemLifecycleProvider, WindowsSystemLifecycleProvider>();
            }
            else
                throw new PlatformNotSupportedException(
                    "Only Windows6.0.6000 or later is supported."
                );
        }
        else if (OperatingSystem.IsLinux())
        {
            services.AddSingleton<IForegroundWindowMonitor, LinuxForegroundWindowMonitor>();
            services.AddSingleton<IExecutableMetadataProvider, LinuxExecutableMetadataProvider>();
            services.AddSingleton<IIdleTimeProvider, LinuxIdleTimeProvider>();
            services.AddSingleton<ISystemLifecycleProvider, LinuxSystemLifecycleProvider>();
        }
        else
            throw new PlatformNotSupportedException("Only Windows and Linux are supported.");

        return services;
    }
}
