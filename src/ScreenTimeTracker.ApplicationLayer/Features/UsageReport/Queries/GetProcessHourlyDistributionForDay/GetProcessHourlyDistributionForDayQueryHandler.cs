using Mediator;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetProcessHourlyDistributionForDay;

public class GetProcessHourlyDistributionForDayQueryHandler(IUsageReportReadService readService) : IQueryHandler<GetProcessHourlyDistributionForDayQuery, IDictionary<int, long>>
{
    private readonly IUsageReportReadService _readService = readService;

    public async ValueTask<IDictionary<int, long>> Handle(GetProcessHourlyDistributionForDayQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetProcessHourlyDistributionForDay(
            query.ProcessId, query.Date, cancellationToken);
    }
}
