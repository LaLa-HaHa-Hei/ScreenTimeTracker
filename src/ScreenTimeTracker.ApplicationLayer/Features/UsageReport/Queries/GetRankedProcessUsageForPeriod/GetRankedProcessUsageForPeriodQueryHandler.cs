using Mediator;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;

public class GetRankedProcessUsageForPeriodQueryHandler(IUsageReportReadService readService) : IQueryHandler<GetRankedProcessUsageForPeriodQuery, IEnumerable<ProcessUsageRankEntry>>
{
    private readonly IUsageReportReadService _readService = readService;

    public async ValueTask<IEnumerable<ProcessUsageRankEntry>> Handle(GetRankedProcessUsageForPeriodQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetRankedProcessUsageForPeriod(
            query.StartDate, query.EndDate, query.TopN, query.ExcludedProcessIds, cancellationToken);
    }
}
