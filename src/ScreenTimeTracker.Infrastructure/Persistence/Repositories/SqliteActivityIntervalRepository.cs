using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;
using ScreenTimeTracker.Infrastructure.Persistence.Mappers;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Repositories
{
    public class SqliteActivityIntervalRepository(ScreenTimeDbContext dbContext) : IActivityIntervalRepository
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public async Task AddAsync(ActivityInterval activityInterval)
        {
            ActivityIntervalEntity entityToAdd = ActivityIntervalMapper.Map(activityInterval);
            entityToAdd.ProcessInfoEntity = null!;
            _dbContext.ActivityIntervals.Add(entityToAdd);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(IEnumerable<ActivityInterval> activityInterval)
        {
            var ids = activityInterval.Select(i => i.Id).ToList();
            var entitiesToUpdate = await _dbContext.ActivityIntervals
                .Where(e => ids.Contains(e.Id))
                .ToListAsync();
            var entitiesDict = entitiesToUpdate.ToDictionary(e => e.Id);

            foreach (var interval in activityInterval)
            {
                if (entitiesDict.TryGetValue(interval.Id, out ActivityIntervalEntity? entityToUpdate))
                {
                    entityToUpdate.Timestamp = interval.Timestamp;
                    entityToUpdate.ProcessInfoEntityId = interval.TrackedProcess.Id;
                    entityToUpdate.DurationMilliseconds = (int)interval.Duration.TotalMilliseconds;
                }
                else
                {
                    throw new InvalidOperationException($"Activity interval with id {interval.Id} not found.");
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task RemoveRangeAsync(IEnumerable<ActivityInterval> intervals)
        {
            var ids = intervals.Select(x => x.Id).ToList();

            await _dbContext.ActivityIntervals
                .Where(x => ids.Contains(x.Id))
                .ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<ActivityInterval>> GetByTimestampBeforeAsync(DateTime timestamp)
        {
            var entities = await _dbContext.ActivityIntervals
                .AsNoTracking()
                .Include(e => e.ProcessInfoEntity)
                .Where(x => x.Timestamp < timestamp)
                .ToListAsync();
            return entities.Select(ActivityIntervalMapper.Map);
        }

        public async Task<IEnumerable<ActivityInterval>> GetNonIdleByTimestampAfterAsync(DateTime timestamp)
        {
            var entities = await _dbContext.ActivityIntervals
                .AsNoTracking()
                .Include(e => e.ProcessInfoEntity)
                .Where(x => x.Timestamp >= timestamp && x.ProcessInfoEntityId != ProcessInfo.IdleProcessId)
                .ToListAsync();
            return entities.Select(ActivityIntervalMapper.Map);
        }
    }
}
