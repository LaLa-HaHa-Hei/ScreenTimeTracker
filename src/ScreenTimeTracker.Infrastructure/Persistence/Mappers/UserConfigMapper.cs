using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Mappers
{
    internal class UserConfigMapper
    {
        public static UserConfiguration Map(UserConfigEntity entity)
        {
            return new UserConfiguration(
                TrackerSettingsMapper.Map(entity.Tracker),
                AggregationSettingsMapper.Map(entity.Aggregation)
            );
        }

        public static UserConfigEntity Map(UserConfiguration domain)
        {
            return new UserConfigEntity
            {
                Tracker = TrackerSettingsMapper.Map(domain.Tracker),
                Aggregation = AggregationSettingsMapper.Map(domain.Aggregation)
            };
        }
    }
}
