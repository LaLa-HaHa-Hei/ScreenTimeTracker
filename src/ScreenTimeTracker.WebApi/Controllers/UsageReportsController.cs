using Microsoft.AspNetCore.Mvc;
using ScreenTimeTracker.Application.DTOs;
using ScreenTimeTracker.Application.Interfaces;

namespace ScreenTimeTracker.WebApi.Controllers;

[ApiController]
[Route("api/usage-reports")]
public class UsageReportsController(IUsageReportQueries usageReportQueries) : ControllerBase
{
    private readonly IUsageReportQueries _usageReportQueries = usageReportQueries;

    [HttpGet("ranks/processes")]
    public async Task<IEnumerable<ProcessUsageRankEntry>> ProcessUsageRankEntryForPeriod([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] int topN = 10, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return await _usageReportQueries.GetRankedProcessUsageForPeriodAsync(startDate, endDate, topN, excludedProcessIds);
    }

    [HttpGet("summaries/hourly")]
    public async Task<IDictionary<int, long>> TotalHourlyUsage([FromQuery] DateOnly date, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return await _usageReportQueries.GetTotalHourlyUsageForDayAsync(date, excludedProcessIds);
    }

    [HttpGet("summaries/daily")]
    public async Task<IDictionary<DateOnly, long>> TotalDailyUsage([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] IEnumerable<Guid>? excludedProcessIds = null)
    {
        return await _usageReportQueries.GetTotalDailyUsageForPeriodAsync(startDate, endDate, excludedProcessIds);
    }
}
