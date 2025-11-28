using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.DomainLayer.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.Options;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;
using ScreenTimeTracker.Infrastructure.Persistence.Repositories;
using ScreenTimeTracker.Infrastructure.Persistence;
using ScreenTimeTracker.ApplicationLayer.Common.Interfaces;
using ScreenTimeTracker.Infrastructure.OS.Windows;

namespace ScreenTimeTracker.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置
            services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
            services.Configure<UserConfigStorageOptions>(configuration.GetSection(UserConfigStorageOptions.SectionName));

            // 持久化服务
            services.AddScoped<IUserConfigurationRepository, JsonUserConfigurationRepository>();
            services.AddDbContext<ScreenTimeDbContext>((serviceProvider, options) =>
            {
                var persistenceOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

                options.UseSqlite($"Data Source={persistenceOptions.DBFilePath}");
            });
            services.AddHostedService<DbInitializationService>();
            services.AddScoped<IActivityIntervalRepository, SqliteActivityIntervalRepository>();
            services.AddScoped<IHourlySummaryRepository, SqliteHourlySummaryRepository>();
            services.AddScoped<IProcessInfoRepository, SqliteProcessInfoRepository>();

            // 操作系统服务
            services.AddSingleton<IForegroundWindowService, ForegroundWindowService>();
            services.AddSingleton<IIdleTimeProvider, IdleTimeProvider>();
            services.AddSingleton<IExecutableMetadataProvider, ExecutableMetadataProvider>();
            services.AddSingleton<IStartupManager, StartupManager>();


            return services;
        }
    }
}