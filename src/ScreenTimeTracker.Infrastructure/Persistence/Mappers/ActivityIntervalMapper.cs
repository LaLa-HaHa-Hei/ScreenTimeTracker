using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Mappers
{
    internal class ActivityIntervalMapper
    {
        public static ActivityInterval Map(ActivityIntervalEntity entity)
        {
            return ActivityInterval.Reconstitute(
                entity.Id,
                entity.Timestamp,
                ProcessInfoMapper.Map(entity.ProcessInfoEntity),
                TimeSpan.FromMilliseconds(entity.DurationMilliseconds)
            );
        }

        public static ActivityIntervalEntity Map(ActivityInterval domain)
        {
            return new ActivityIntervalEntity
            {
                Id = domain.Id,
                Timestamp = domain.Timestamp,
                ProcessInfoEntityId = domain.TrackedProcess.Id,
                ProcessInfoEntity = ProcessInfoMapper.Map(domain.TrackedProcess),
                DurationMilliseconds = (int)domain.Duration.TotalMilliseconds
            };
        }
    }
}
