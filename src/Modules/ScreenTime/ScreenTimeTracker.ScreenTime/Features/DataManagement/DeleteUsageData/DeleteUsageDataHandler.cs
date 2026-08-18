using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.DeleteUsageData;

public class DeleteUsageDataHandler(ScreenTimeDbContext context)
    : IRequestHandler<DeleteUsageDataCommand>
{
    public async ValueTask<Unit> Handle(
        DeleteUsageDataCommand request,
        CancellationToken cancellationToken
    )
    {
        var settings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        var dayCutoffHour = settings.TimeBoundary.DayCutoffHour;
        var timeRange = LogicalDay.CalculateUtcWindow(
            request.StartDate,
            request.EndDate,
            settings.TimeBoundary.DayCutoffHour,
            request.TimeZoneInfo
        );

        await context
            .AppUsageSessions.Where(x => timeRange.Start < x.EndTime && x.StartTime < timeRange.End)
            .ExecuteDeleteAsync(cancellationToken);

        return Unit.Value;
    }
}
