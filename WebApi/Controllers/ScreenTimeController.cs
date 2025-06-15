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
    public class ScreenTimeController(ScreenTimeContext context, ILogger<ScreenTimeController> logger) : ControllerBase
    {
        private readonly ScreenTimeContext _context = context;
        private readonly ILogger<ScreenTimeController> _logger = logger;
        private const int DefaultProcessLimit = 10; // 默认限制返回的进程数量
        private const int MaxQueryDays = 35; // 最大查询天数常量

        /// <summary>
        /// 删除 HourlyUsages 和 DailyUsages 中指定日期以前的数据
        /// </summary>
        /// <param name="date">要保留的最早日期（不删除该日期及之后的数据）</param>
        [HttpDelete("cleanup/before")]
        public async Task<IActionResult> DeleteUsagesBeforeDate([FromQuery, Required] DateOnly date)
        {
            int deletedDailyCount = await _context.DailyUsages
                .Where(x => x.Date < date)
                .ExecuteDeleteAsync();

            int deletedHourlyCount = await _context.HourlyUsages
                .Where(x => x.Date < date)
                .ExecuteDeleteAsync();

            int totalDeleted = deletedDailyCount + deletedHourlyCount;

            _logger.LogInformation("已删除 {Count} 条早于 {Date} 的使用记录", totalDeleted, date);

            return Ok(new { DeletedCount = totalDeleted });
        }

        /// <summary>
        /// 获取指定进程在某天的24小时使用情况
        /// </summary>
        [HttpGet("processes/{processName}/daily")]
        public async Task<ActionResult<IEnumerable<long>>> GetProcessDailyUsage(
            string processName,
            [FromQuery, Required] DateOnly date)
        {
            // 进程名称有可能是空格，所以允许空字符串

            var hourlyData = await _context.HourlyUsages
                .Where(h => h.Date == date && h.ProcessName == processName)
                .AsNoTracking()
                .ToDictionaryAsync(h => h.Hour, h => h.DurationMs);

            var hourlyDurationMs = new long[24];


            // 填充24小时数据
            for (int hour = 0; hour < 24; hour++)
                hourlyDurationMs[hour] = hourlyData.GetValueOrDefault(hour, 0);

            return Ok(hourlyDurationMs);
        }

        /// <summary>
        /// 获取指定进程在日期范围内的每日使用情况
        /// </summary>
        [HttpGet("processes/{processName}/range")]
        public async Task<ActionResult<IEnumerable<DateUsage>>> GetProcessUsageRange(
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
        /// 获取所有进程在日期范围内的每日使用情况，从时长长倒短
        /// <param name="limit">使用时间由多到少前 N 个，为非正数时不限个数</param>
        /// </summary>
        [HttpGet("processes/range")]
        public async Task<ActionResult<IEnumerable<ProcessUsage>>> GetTopProcessesUsageRange(
            [FromQuery, Required] DateOnly startDate,
            [FromQuery, Required] DateOnly endDate,
            [FromQuery] int limit = DefaultProcessLimit)
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

            return Ok(processUsages);
        }


        /// <summary>
        /// 获取指定日期内共24小时每小时内所有进程使用情况
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpGet("summary/daily")]
        public async Task<ActionResult<IEnumerable<long>>> GetDailyUsageSummary(
            [FromQuery, Required] DateOnly date)
        {
            var hourlyData = await _context.HourlyUsages
                .Where(h => h.Date == date)
                .GroupBy(x => x.Hour)
                .Select(g => new 
                {
                    Hour = g.Key,
                    DurationMs = g.Sum(x => x.DurationMs)
                })
                .AsNoTracking()
                .ToDictionaryAsync(h => h.Hour, h => h.DurationMs);

            var hourlyDurationMs = new long[24];

            // 填充24小时数据
            for (int hour = 0; hour < 24; hour++)
                hourlyDurationMs[hour] = hourlyData.GetValueOrDefault(hour, 0);

            return Ok(hourlyDurationMs);
        }

        /// <summary>
        /// 获取指定日期范围内的所有进程使用情况
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        [HttpGet("summary/range")]
        public async Task<ActionResult<IEnumerable<DateUsage>>> GetUsageSummaryRange(
            [FromQuery, Required] DateOnly startDate,
            [FromQuery, Required] DateOnly endDate)
        {
            var validationResult = ValidateDateRange(startDate, endDate);
            if (validationResult != null)
                return validationResult;

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

            return Ok(dailySummary);
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
