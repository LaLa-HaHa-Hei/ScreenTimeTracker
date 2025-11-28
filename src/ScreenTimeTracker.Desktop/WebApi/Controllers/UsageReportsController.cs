using Mediator;
using Microsoft.AspNetCore.Mvc;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalDailyUsageForPeriod;
using ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetTotalHourlyUsageForDay;

namespace ScreenTimeTracker.Desktop.WebApi.Controllers;

[ApiController]
[Route("api/usage-reports")]
public class UsageReportsController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpGet("ranks/processes")]
    public async Task<IEnumerable<ProcessUsageRankEntry>> ProcessUsageRankEntryForPeriod([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] int topN = 10, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return await _mediator.Send(new GetRankedProcessUsageForPeriodQuery(startDate, endDate, topN, excludedProcessIds));
    }

    [HttpGet("summaries/hourly")]
    public async Task<IDictionary<int, long>> TotalHourlyUsage([FromQuery] DateOnly date, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return await _mediator.Send(new GetTotalHourlyUsageForDayQuery(date, excludedProcessIds));
    }

    [HttpGet("summaries/daily")]
    public async Task<IDictionary<DateOnly, long>> TotalDailyUsage([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return await _mediator.Send(new GetTotalDailyUsageForPeriodQuery(startDate, endDate, excludedProcessIds));
    }
}
