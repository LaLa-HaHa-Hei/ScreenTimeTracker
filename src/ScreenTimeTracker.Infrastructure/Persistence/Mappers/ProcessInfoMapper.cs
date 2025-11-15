using ScreenTimeTracker.Domain.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Mappers
{
    internal class ProcessInfoMapper
    {
        public static ProcessInfo Map(ProcessInfoEntity entity)
        {
            return ProcessInfo.Reconstitute(
                id: entity.Id,
                name: entity.Name,
                alias: entity.Alias,
                autoUpdate: entity.AutoUpdate,
                lastAutoUpdated: entity.LastAutoUpdated,
                executablePath: entity.ExecutablePath,
                iconPath: entity.IconPath,
                description: entity.Description);
        }

        public static ProcessInfoEntity Map(ProcessInfo domain)
        {
            return new ProcessInfoEntity
            {
                Id = domain.Id,
                Name = domain.Name,
                Alias = domain.Alias,
                AutoUpdate = domain.AutoUpdate,
                LastAutoUpdated = domain.LastAutoUpdated,
                ExecutablePath = domain.ExecutablePath,
                IconPath = domain.IconPath,
                Description = domain.Description
            };
        }
    }
}
