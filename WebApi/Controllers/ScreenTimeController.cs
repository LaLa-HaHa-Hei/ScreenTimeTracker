using System.ComponentModel.DataAnnotations;
using Data;
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
                HourlyUsages = new long[24]
            };

            // 填充24小时数据
            for (int hour = 0; hour < 24; hour++)
                response.HourlyUsages[hour] = hourlyData.TryGetValue(hour, out var duration) ? duration : 0;

            return Ok(response);
        }

        /// <summary>
        /// 获取指定日期的所有进程使用情况
        /// </summary>
        /// <param name="limit">使用时间由多到少前 N 个，为 0 时不限个数</param>
        /// <returns></returns>
        [HttpGet("{date}/summary")]
        public async Task<ActionResult<IEnumerable<ProcessUsage>>> GetUsageByDate(DateOnly date, [FromQuery] int limit = 10)
        {
            IQueryable<ProcessUsage> query = _context.DailyUsages
                .Where(x => x.Date == date)
                .OrderByDescending(x => x.DurationMs)
                .Select(x => new ProcessUsage
                {
                    ProcessName = x.ProcessName,
                    DurationMs = x.DurationMs,
                })
                .OrderByDescending(x => x.DurationMs);

            // limit = 0 时表示不限制返回数量
            if (limit > 0)
                query = query.Take(limit);

            return Ok(await query.ToListAsync());
        }

        /// <summary>
        /// 获取指定日期范围内的所有进程使用情况
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="limit">使用时间由多到少前 N 个，为 0 时不限个数</param>
        /// <returns></returns>
        [HttpGet("summary")]
        public async Task<ActionResult<IEnumerable<ProcessUsage>>> GetUsageByDateRange(
            [FromQuery, Required] DateOnly startDate,
            [FromQuery, Required] DateOnly endDate,
            [FromQuery] int limit = 10)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);

            IQueryable<ProcessUsage> query = _context.DailyUsages
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .GroupBy(x => x.ProcessName)
                .Select(g => new ProcessUsage
                {
                    ProcessName = g.Key,
                    DurationMs = g.Sum(x => x.DurationMs),
                })
                .OrderByDescending(x => x.DurationMs);

            // limit = 0 时表示不限制返回数量
            if (limit > 0)
                query = query.Take(limit);

            return Ok(await query.ToListAsync());
        }
    }
}
