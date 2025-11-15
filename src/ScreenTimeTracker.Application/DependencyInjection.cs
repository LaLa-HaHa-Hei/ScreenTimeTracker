using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ScreenTimeTracker.Application.Configuration;
using ScreenTimeTracker.Application.Services;

namespace ScreenTimeTracker.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
        {
            // 配置
            services.Configure<TrackerOptions>(
                configuration.GetSection(TrackerOptions.SectionName));
            services.Configure<AggregationOptions>(
                configuration.GetSection(AggregationOptions.SectionName));

            // 服务
            services.AddSingleton<TrackerService>();
            services.AddSingleton<AggregationService>();
            services.AddScoped<ProcessManagementService>();

            return services;
        }
    }
}