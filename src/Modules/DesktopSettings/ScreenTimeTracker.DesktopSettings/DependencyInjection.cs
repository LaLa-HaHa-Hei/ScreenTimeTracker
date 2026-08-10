using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.BuildingBlocks.Persistence;
using ScreenTimeTracker.BuildingBlocks.Persistence.Interceptors;
using ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;
using ScreenTimeTracker.DesktopSettings.Infrastructure.OS;
using ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

namespace ScreenTimeTracker.DesktopSettings;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDesktopSettingsServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        services.AddScoped<DispatchDomainEventsInterceptor>();

        services.AddDbContext<DesktopSettingsDbContext>(
            (serviceProvider, options) =>
            {
                var interceptor =
                    serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>();
                var persistenceOptions = serviceProvider
                    .GetRequiredService<IOptions<DatabaseOptions>>()
                    .Value;
                options
                    .UseSqlite(
                        $"Data Source={persistenceOptions.DBFilePath}",
                        x => x.MigrationsHistoryTable("__EFMigrationsHistory_DesktopSettings")
                    )
                    .AddInterceptors(interceptor);
            }
        );
        services.AddHostedService<DesktopSettingsDbMigrationService>();

        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IStartupManager, WindowsStartupManager>();
        }
        else if (OperatingSystem.IsLinux())
        {
            services.AddSingleton<IStartupManager, LinuxStartupManager>();
        }
        else
        {
            throw new PlatformNotSupportedException(
                "DesktopSettings module is only supported on Windows or Linux."
            );
        }

        return services;
    }
}
