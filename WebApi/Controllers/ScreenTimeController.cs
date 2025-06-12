using System.ComponentModel.DataAnnotations;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi.DTOs.ScreenTime;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/screen-time")]
    public class ScreenTimeController : ControllerBase
    {
        private readonly ScreenTimeContext _context;
        private readonly ILogger<ScreenTimeController> _logger;
        private const int DefaultLimit = 10; // 默认限制返回的进程数量
        private const int MaxQueryDays = 35; // 最大查询天数常量

        public ScreenTimeController(ScreenTimeContext context, ILogger<ScreenTimeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// 获取指定进程在某天的24小时使用情况
        /// GET /api/screen-time/processes/{processName}/hourly?date=2024-01-15
        /// </summary>
        [HttpGet("processes/{processName}/hourly")]
        public async Task<ActionResult<ProcessDailyUsageResponse>> GetProcessHourlyUsage(
            string processName,
            [FromQuery, Required] DateOnly date)
        {
            // 进程名称有可能是空格，所以允许空字符串

            var hourlyData = await _context.HourlyUsages
                .Where(h => h.Date == date && h.ProcessName == processName)
                .AsNoTracking()
                .ToDictionaryAsync(h => h.Hour, h => h.DurationMs);

            var response = new ProcessDailyUsageResponse
            {
                HourlyDurationMs = new long[24]
            };

            // 填充24小时数据
            for (int hour = 0; hour < 24; hour++)
                response.HourlyDurationMs[hour] = hourlyData.GetValueOrDefault(hour, 0);

            return Ok(response);
        }

        /// <summary>
        /// 获取指定进程在日期范围内的每日使用情况
        /// GET /api/screen-time/processes/{processName}/daily?startDate=2024-01-01&endDate=2024-01-31
        /// </summary>
        [HttpGet("processes/{processName}/daily")]
        public async Task<ActionResult<IEnumerable<DateUsage>>> GetProcessDailyUsage(
            string processName,
            [FromQuery, Required] DateOnly startDate,
            [FromQuery, Required] DateOnly endDate)
        {
            // 进程名称有可能是空格，所以允许空字符串

            var validationResult = ValidateDateRange(startDate, endDate);
            if (validationResult != null)
                return validationResult;

            var dailyUsageDict = await _context.DailyUsages
                .Where(x => x.ProcessName == processName && x.Date >= startDate && x.Date <= endDate)
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Date, x => x.DurationMs);

            // 生成完整日期范围并填充数据
            var response = GenerateDateRange(startDate, endDate)
                .Select(date => new DateUsage
                {
                    Date = date,
                    DurationMs = dailyUsageDict.GetValueOrDefault(date, 0)
                })
                .ToList();

            return Ok(response);
        }

        /// <summary>
        /// 获取指定日期范围内的所有进程使用情况
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="limit">使用时间由多到少前 N 个，为非正数时不限个数</param>
        /// <returns></returns>
        [HttpGet("summary/range")]
        public async Task<ActionResult<ProcessUsageByDateRangeResponse>> GetUsageSummary(
            [FromQuery, Required] DateOnly startDate,
            [FromQuery, Required] DateOnly endDate,
            [FromQuery] int limit = DefaultLimit)
        {
            var validationResult = ValidateDateRange(startDate, endDate);
            if (validationResult != null)
                return validationResult;

            // 每个进程在这段时间内使用情况和
            IQueryable<ProcessUsage> processQuery = _context.DailyUsages
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .GroupBy(x => x.ProcessName)
                .Select(g => new ProcessUsage
                {
                    ProcessName = g.Key,
                    DurationMs = g.Sum(x => x.DurationMs)
                })
                .OrderByDescending(x => x.DurationMs)
                .AsNoTracking();
            if (limit > 0)
                processQuery = processQuery.Take(limit);

            var processUsages = await processQuery.ToListAsync();

            // 每一天所有进程使用情况统计
            var dailyUsageDict = await _context.DailyUsages
                .Where(x => x.Date >= startDate && x.Date <= endDate)
                .GroupBy(x => x.Date)
                .Select(g => new DateUsage
                {
                    Date = g.Key,
                    DurationMs = g.Sum(x => x.DurationMs)
                })
                .AsNoTracking()
                .ToDictionaryAsync(x => x.Date, x => x.DurationMs);

            // 生成完整的日期范围并填充数据
            var dailySummary = GenerateDateRange(startDate, endDate)
                .Select(date => new DateUsage
                {
                    Date = date,
                    DurationMs = dailyUsageDict.GetValueOrDefault(date, 0)
                })
                .ToList();

            return Ok(new ProcessUsageByDateRangeResponse
            {
                ProcessUsages = processUsages,
                DailyUsageSummary = dailySummary
            });
        }

        /// <summary>
        /// 生成日期范围
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>日期序列</returns>
        private static IEnumerable<DateOnly> GenerateDateRange(DateOnly startDate, DateOnly endDate)
        {
            var daysDiff = endDate.DayNumber - startDate.DayNumber + 1;
            return Enumerable.Range(0, daysDiff)
                .Select(offset => startDate.AddDays(offset));
        }

        /// <summary>
        /// 验证日期范围
        /// </summary>
        /// <param name="startDate">开始日期</param>
        /// <param name="endDate">结束日期</param>
        /// <returns>验证失败时返回BadRequest，验证通过返回null</returns>
        private BadRequestObjectResult? ValidateDateRange(DateOnly startDate, DateOnly endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("开始日期不能晚于结束日期");
            }

            var daysDiff = endDate.DayNumber - startDate.DayNumber + 1;
            if (daysDiff > MaxQueryDays)
            {
                return BadRequest($"日期范围不能超过 {MaxQueryDays} 天");
            }

            return null;
        }
    }
}
