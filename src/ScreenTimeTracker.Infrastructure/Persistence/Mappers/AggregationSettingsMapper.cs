using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Mappers
{
    internal class AggregationSettingsMapper
    {
        public static AggregationSettings Map(AggregationSettingsEntity entity)
        {
            return new AggregationSettings(
                entity.PollingInterval
            );
        }

        public static AggregationSettingsEntity Map(AggregationSettings domain)
        {
            return new AggregationSettingsEntity
            {
                PollingInterval = domain.PollingInterval,
            };
        }
    }
}
