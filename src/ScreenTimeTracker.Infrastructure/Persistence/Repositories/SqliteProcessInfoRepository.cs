using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Common.Exceptions;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;
using ScreenTimeTracker.Infrastructure.Persistence.Mappers;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Repositories
{
    public class SqliteProcessInfoRepository(ScreenTimeDbContext dbContext) : IProcessInfoRepository
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public async Task AddAsync(ProcessInfo processInfo)
        {
            _dbContext.ProcessInfos.Add(ProcessInfoMapper.Map(processInfo));
            await _dbContext.SaveChangesAsync();
        }

        public async Task<ProcessInfo?> GetByIdAsync(Guid id)
        {
            ProcessInfoEntity? entity = await _dbContext.ProcessInfos.FindAsync(id);
            return entity is null ? null : ProcessInfoMapper.Map(entity);
        }

        public async Task<ProcessInfo?> GetByNameAsync(string name)
        {
            ProcessInfoEntity? entity = await _dbContext.ProcessInfos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name.ToLower() == name.ToLower());
            return entity is null ? null : ProcessInfoMapper.Map(entity);
        }

        public async Task UpdateAsync(ProcessInfo processInfo)
        {
            ProcessInfoEntity? entityToUpdate = await _dbContext.ProcessInfos.FindAsync(processInfo.Id)
                ?? throw new ArgumentException($"ProcessInfo with Id {processInfo.Id} not found.");

            entityToUpdate.Alias = processInfo.Alias;
            entityToUpdate.AutoUpdate = processInfo.AutoUpdate;
            entityToUpdate.LastAutoUpdated = processInfo.LastAutoUpdated;
            entityToUpdate.ExecutablePath = processInfo.ExecutablePath;
            entityToUpdate.IconPath = processInfo.IconPath;
            entityToUpdate.Description = processInfo.Description;

            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            if (id == ProcessInfo.IdleProcessId || id == ProcessInfo.UnknownProcessId)
                throw new InvalidOperationException($"Cannot delete the idle or unknown process.");
            ProcessInfoEntity? entityToDelete = await _dbContext.ProcessInfos.FindAsync(id)
                ?? throw new NotFoundException($"ProcessInfo with Id {id} not found.");

            _dbContext.ProcessInfos.Remove(entityToDelete);
            await _dbContext.SaveChangesAsync();
        }
    }
}
