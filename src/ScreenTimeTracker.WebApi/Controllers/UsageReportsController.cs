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
    public async Task<IEnumerable<ProcessUsageRankEntry>> ProcessUsageRankEntryForPeriod([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate, [FromQuery] int topN = 10)
    {
        return await _usageReportQueries.GetRankedProcessUsageForPeriodAsync(startDate, endDate, topN);
    }

    [HttpGet("summaries/hourly")]
    public async Task<IDictionary<int, TimeSpan>> TotalHourlyUsageForDay([FromQuery] DateOnly date)
    {
        return await _usageReportQueries.GetTotalHourlyUsageForDayAsync(date);
    }

    [HttpGet("summaries/daily")]
    public async Task<IDictionary<DateOnly, TimeSpan>> TotalDailyUsageForPeriod([FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        return await _usageReportQueries.GetTotalDailyUsageForPeriodAsync(startDate, endDate);
    }
}
