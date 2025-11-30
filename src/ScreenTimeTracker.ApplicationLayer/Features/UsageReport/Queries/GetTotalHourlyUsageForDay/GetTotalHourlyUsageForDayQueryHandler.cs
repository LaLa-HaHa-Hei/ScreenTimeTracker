using Mediator;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalHourlyUsageForDay;

public class GetTotalHourlyUsageForDayQueryHandler(IUsageReportReadService readService) : IQueryHandler<GetTotalHourlyUsageForDayQuery, IDictionary<int, long>>
{
    private readonly IUsageReportReadService _readService = readService;

    public async ValueTask<IDictionary<int, long>> Handle(GetTotalHourlyUsageForDayQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetTotalHourlyUsageForDay(
            query.Date, query.ExcludedProcessIds, cancellationToken);
    }
}
