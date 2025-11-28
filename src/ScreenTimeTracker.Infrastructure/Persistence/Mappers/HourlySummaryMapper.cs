using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Mappers
{
    internal class HourlySummaryMapper
    {
        public static HourlySummary Map(HourlySummaryEntity entity)
        {
            return HourlySummary.Reconstitute(
                ProcessInfoMapper.Map(entity.ProcessInfoEntity),
                entity.Hour,
                TimeSpan.FromMilliseconds(entity.TotalDurationMilliseconds));
        }

        public static HourlySummaryEntity Map(HourlySummary domain)
        {
            return new HourlySummaryEntity
            {
                ProcessInfoEntityId = ProcessInfoMapper.Map(domain.TrackedProcess).Id,
                ProcessInfoEntity = ProcessInfoMapper.Map(domain.TrackedProcess),
                Hour = domain.Hour,
                TotalDurationMilliseconds = (int)domain.TotalDuration.TotalMilliseconds
            };
        }
    }
}
