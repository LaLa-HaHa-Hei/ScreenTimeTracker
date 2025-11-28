using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessIconPathById;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries.ProcessInfos;

public class GetProcessIconPathByIdQueryHandler(ScreenTimeDbContext dbContext) : IQueryHandler<GetProcessIconPathByIdQuery, string?>
{
    private readonly ScreenTimeDbContext _dbContext = dbContext;

    public async ValueTask<string?> Handle(GetProcessIconPathByIdQuery query, CancellationToken cancellationToken)
    {
        var processInfo = await _dbContext.ProcessInfos.FindAsync([query.Id], cancellationToken);
        return processInfo?.IconPath;
    }
}
