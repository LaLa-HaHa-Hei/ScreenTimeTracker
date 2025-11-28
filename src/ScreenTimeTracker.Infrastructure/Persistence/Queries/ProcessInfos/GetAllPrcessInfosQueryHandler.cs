using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetAllProcessInfos;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.ProcessInfos;

public class GetAllPrcessInfosQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetAllProcessInfosQuery, IEnumerable<ProcessInfoDto>>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<IEnumerable<ProcessInfoDto>> Handle(GetAllProcessInfosQuery query, CancellationToken cancellationToken)
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
}
