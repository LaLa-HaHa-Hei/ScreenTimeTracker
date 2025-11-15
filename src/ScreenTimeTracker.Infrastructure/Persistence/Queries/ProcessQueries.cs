using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ScreenTimeTracker.Application.DTOs;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries
{
    public class ProcessQueries(ScreenTimeDbContext dbContext) : IProcessQueries
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public async Task<IEnumerable<ProcessInfoDto>> GetAllProcessesAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.ProcessInfos
                .AsNoTracking()
                .Select(p => new ProcessInfoDto(
                    Id: p.Id,
                    Name: p.Name,
                    Alias: p.Alias,
                    AutoUpdate: p.AutoUpdate,
                    LastAutoUpdated: p.LastAutoUpdated.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
                    ExecutablePath: p.ExecutablePath,
                    IconPath: p.IconPath,
                    Description: p.Description
                ))
                .ToListAsync(cancellationToken);
        }

        public async Task<string?> GetProcessIconByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var processInfo = await _dbContext.ProcessInfos.FindAsync([id], cancellationToken);
            return processInfo?.IconPath;
        }

        public async Task<ProcessInfoDto?> GetProcessByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var processInfo = await _dbContext.ProcessInfos.FindAsync([id], cancellationToken);
            return processInfo is null
                ? null
                : new ProcessInfoDto(
                    Id: processInfo.Id,
                    Name: processInfo.Name,
                    Alias: processInfo.Alias,
                    AutoUpdate: processInfo.AutoUpdate,
                    LastAutoUpdated: processInfo.LastAutoUpdated.ToString("yyyy-MM-dd"),
                    ExecutablePath: processInfo.ExecutablePath,
                    IconPath: processInfo.IconPath,
                    Description: processInfo.Description
                );
        }
    }
}
