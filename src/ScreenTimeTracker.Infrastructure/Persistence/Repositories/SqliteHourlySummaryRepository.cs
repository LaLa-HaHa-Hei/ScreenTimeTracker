using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.Domain.Entities;
using ScreenTimeTracker.Domain.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;
using ScreenTimeTracker.Infrastructure.Persistence.Mappers;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Repositories
{
    public class SqliteHourlySummaryRepository(ScreenTimeDbContext dbContext) : IHourlySummaryRepository
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public async Task AddAsync(HourlySummary hourlySummary)
        {
            HourlySummaryEntity entityToAdd = HourlySummaryMapper.Map(hourlySummary);
            entityToAdd.ProcessInfoEntity = null!;
            _dbContext.HourlySummaries.Add(entityToAdd);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(HourlySummary hourlySummary)
        {
            HourlySummaryEntity? entityToUpdate = await _dbContext.HourlySummaries.FindAsync(hourlySummary.TrackedProcess.Id, hourlySummary.Hour)
                ?? throw new InvalidOperationException($"HourlySummary entity with ProcessId {hourlySummary.TrackedProcess.Id} and Hour {hourlySummary.Hour} not found.");

            entityToUpdate.TotalDurationMilliseconds = (int)hourlySummary.TotalDuration.TotalMilliseconds;
            await _dbContext.SaveChangesAsync();
        }

        public async Task<HourlySummary?> GetByProcessAndHourAsync(ProcessInfo process, DateTime hour)
        {
            var entity = await _dbContext.HourlySummaries
                .AsNoTracking()
                .Include(e => e.ProcessInfoEntity)
                .FirstOrDefaultAsync(e => e.ProcessInfoEntityId == process.Id && e.Hour == hour);
            return entity is null ? null : HourlySummaryMapper.Map(entity);
        }
    }
}
