using System.ComponentModel.DataAnnotations;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.ScreenTime;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreenTimeController(ScreenTimeContext context, ILogger<ScreenTimeController> logger) : ControllerBase
    {
        private readonly ScreenTimeContext _context = context;
        private readonly ILogger<ScreenTimeController> _logger = logger;

        // 获取指定日期和进程名称的24小时使用情况
        [HttpGet("{date}/processName/{processName}/hourly")]
        public async Task<ActionResult<ProcessDailyUsageResponse>> GetProcessDailyUsage(DateOnly date, string processName)
        {
            var hourlyData = await _context.HourlyUsages
                .Where(h => h.Date == date && h.ProcessName == processName)
                .ToDictionaryAsync(h => h.Hour, h => h.DurationMs);

            var response = new ProcessDailyUsageResponse
            {
                HourlyDurationMs = new long[24]
            };

            // 填充24小时数据
            for (int hour = 0; hour < 24; hour++)
                response.HourlyDurationMs[hour] = hourlyData.TryGetValue(hour, out var duration) ? duration : 0;

            return Ok(response);
        }

        /// <summary>
        /// 获取指定日期的所有进程使用情况
        /// </summary>
        /// <param name="limit">使用时间由多到少前 N 个，为 0 时不限个数</param>
        /// <returns></returns>
        [HttpGet("{date}/summary")]
        public async Task<ActionResult<IEnumerable<ProcessUsageByDateRangeResponse>>> GetUsageByDate(DateOnly date, [FromQuery] int limit = 10)
        {
            var result = await GetUsageByDateRangeInternal(date, date, limit);
            return Ok(result);
        }

        /// <summary>
        /// 获取指定日期范围内的所有进程使用情况
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="limit">使用时间由多到少前 N 个，为 0 时不限个数</param>
        /// <returns></returns>
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<ProcessUsageByDateRangeResponse>>> GetUsageByDateRange(
            [FromQuery, Required] DateOnly startDate,
            [FromQuery, Required] DateOnly endDate,
            [FromQuery] int limit = 10)
        {
            var result = await GetUsageByDateRangeInternal(startDate, endDate, limit);
            return Ok(result);
        }

        private async Task<ProcessUsageByDateRangeResponse> GetUsageByDateRangeInternal(
            DateOnly startDate,
            DateOnly endDate,
            int limit = 10)
        {
            // 每个进程在这段时间内使用情况和
            IQueryable<ProcessUsage> processQuery = _context.DailyUsages
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .GroupBy(x => x.ProcessName)
                .Select(g => new ProcessUsage
                {
                    ProcessName = g.Key,
                    DurationMs = g.Sum(x => x.DurationMs)
                })
                .OrderByDescending(x => x.DurationMs);
            if (limit > 0)
                processQuery = processQuery.Take(limit);
            var processUsages = await processQuery.ToListAsync();
            // 每一天所有进程使用情况和
            var dateList = Enumerable.Range(0, (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .ToList();
            var usageData = await _context.DailyUsages
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .GroupBy(x => x.Date)
                .Select(g => new DailyUsageSummary
                {
                    Date = g.Key,
                    DurationMs = g.Sum(x => x.DurationMs)
                })
                .ToDictionaryAsync(x => x.Date, x => x.DurationMs);
            // 合并完整日期和实际数据
            var dailySummary = dateList
                .Select(date => new DailyUsageSummary
                {
                    Date = date,
                    DurationMs = usageData.TryGetValue(date, out long value) ? value : 0
                })
                .ToList();

            return new ProcessUsageByDateRangeResponse
            {
                ProcessUsages = processUsages,
                DailyUsageSummary = dailySummary
            };
        }
    }
}
