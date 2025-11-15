using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Domain.Interfaces;
using ScreenTimeTracker.Infrastructure.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.Configuration;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;
using ScreenTimeTracker.Infrastructure.Persistence.Queries;
using ScreenTimeTracker.Infrastructure.Persistence.Repositories;

namespace ScreenTimeTracker.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 配置
            services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));

            // sqlite数据库
            services.AddDbContext<ScreenTimeDbContext>((serviceProvider, options) =>
            {
                var persistenceOptions = serviceProvider.GetRequiredService<IOptions<PersistenceOptions>>().Value;

                options.UseSqlite($"Data Source={persistenceOptions.DBFilePath}");
                //    .UseLazyLoadingProxies();
            });
            services.AddScoped<IActivityIntervalRepository, SqliteActivityIntervalRepository>();
            services.AddScoped<IHourlySummaryRepository, SqliteHourlySummaryRepository>();
            services.AddScoped<IProcessInfoRepository, SqliteProcessInfoRepository>();


            // 注册持久化抽象接口
            services.AddScoped<IDbContextInitializer, SqlitePersistenceInitializer>();

            // 注册平台服务
            services.AddSingleton<IForegroundWindowService, Platform.Windows.ForegroundWindowService>();
            services.AddSingleton<IIdleTimeProvider, Platform.Windows.IdleTimeProvider>();
            services.AddSingleton<IExecutableMetadataProvider, Platform.Windows.ExecutableMetadataProvider>();

            // 查询服务
            services.AddScoped<IProcessQueries, ProcessQueries>();
            services.AddScoped<IUsageReportQueries, UsageReportQueries>();

            return services;
        }
    }
}