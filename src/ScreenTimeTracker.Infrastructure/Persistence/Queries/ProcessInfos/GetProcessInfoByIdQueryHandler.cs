using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessInfoById;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.ProcessInfos;

public class GetProcessInfoByIdQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetProcessInfoByIdQuery, ProcessInfoDto?>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<ProcessInfoDto?> Handle(GetProcessInfoByIdQuery query, CancellationToken cancellationToken)
    {

        var processInfo = await _dbContext.ProcessInfos.FindAsync([query.Id], cancellationToken);
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
