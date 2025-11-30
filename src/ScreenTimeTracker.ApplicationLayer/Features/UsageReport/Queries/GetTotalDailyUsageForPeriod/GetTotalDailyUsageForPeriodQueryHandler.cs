using Mediator;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalDailyUsageForPeriod;

public class GetTotalDailyUsageForPeriodQueryHandler(IUsageReportReadService readService) : IQueryHandler<GetTotalDailyUsageForPeriodQuery, IDictionary<DateOnly, long>>
{
    private readonly IUsageReportReadService _readService = readService;

    public async ValueTask<IDictionary<DateOnly, long>> Handle(GetTotalDailyUsageForPeriodQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetTotalDailyUsageForPeriod(
            query.StartDate, query.EndDate, query.ExcludedProcessIds, cancellationToken);
    }
}
