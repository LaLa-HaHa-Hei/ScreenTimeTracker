using Microsoft.Extensions.DependencyInjection;
using ScreenTimeTracker.ApplicationLayer.Common.Services;

namespace ScreenTimeTracker.ApplicationLayer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationLayerServices(this IServiceCollection services)
        {
            // 服务
            services.AddSingleton<TrackerService>();
            services.AddSingleton<AggregationService>();
            services.AddScoped<ProcessInfoManagementService>();

            return services;
        }
    }
}