using Mediator;
using Microsoft.EntityFrameworkCore;
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
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(settings.Regional.TimeZoneId);
        var dayCutoffHour = settings.TimeBoundary.DayCutoffHour;
        var minTime = UsageTimeCalculator.GetLogicalDayStartInUtc(
            request.StartDate,
            dayCutoffHour,
            timeZoneInfo
        );
        var maxTime = UsageTimeCalculator.GetLogicalDayStartInUtc(
            request.EndDate.AddDays(1),
            dayCutoffHour,
            timeZoneInfo
        );

        await context
            .AppUsageSessions.Where(x => minTime <= x.StartTime && x.EndTime < maxTime)
            .ExecuteDeleteAsync(cancellationToken);

        return Unit.Value;
    }
}
