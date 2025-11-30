using Mediator;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessDailyDistributionForPeriod;

public class GetProcessDailyDistributionForPeriodQueryHandler(IUsageReportReadService readService) : IQueryHandler<GetProcessDailyDistributionForPeriodQuery, IDictionary<DateOnly, long>>
{
    private readonly IUsageReportReadService _readService = readService;

    public async ValueTask<IDictionary<DateOnly, long>> Handle(GetProcessDailyDistributionForPeriodQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetProcessDailyDistributionForPeriod(
            query.ProcessId, query.StartDate, query.EndDate, cancellationToken);
    }
}
