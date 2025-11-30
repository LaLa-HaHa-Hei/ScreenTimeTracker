using Mediator;
using Microsoft.AspNetCore.Http;
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
    [ProducesResponseType(typeof(IEnumerable<ProcessUsageRankEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessUsageRankEntryForPeriod([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] int topN = 10, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return Ok(await _mediator.Send(new GetRankedProcessUsageForPeriodQuery(startDate, endDate, topN, excludedProcessIds)));
    }

    [HttpGet("summaries/hourly")]
    [ProducesResponseType(typeof(IDictionary<int, long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TotalHourlyUsage([FromQuery] DateOnly date, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return Ok(await _mediator.Send(new GetTotalHourlyUsageForDayQuery(date, excludedProcessIds)));
    }

    [HttpGet("summaries/daily")]
    [ProducesResponseType(typeof(IDictionary<DateOnly, long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> TotalDailyUsage([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return Ok(await _mediator.Send(new GetTotalDailyUsageForPeriodQuery(startDate, endDate, excludedProcessIds)));
    }
}
