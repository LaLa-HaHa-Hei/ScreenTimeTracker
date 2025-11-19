using ScreenTimeTracker.Application.DTOs; // 允许引用同层的 DTOs

namespace ScreenTimeTracker.Application.Interfaces
{
    public interface IUsageReportQueries
    {

        /// <summary>
        /// 获取指定日期范围内，所有进程的总使用时长，并按时长降序排序。
        /// </summary>
        /// <returns>排序后的程序使用情况列表。</returns>
        Task<IEnumerable<ProcessUsageRankEntry>> GetRankedProcessUsageForPeriodAsync(DateOnly startDate, DateOnly endDate, int topN = 10, IEnumerable<Guid>? excludedProcessIds = null);

        /// <summary>
        /// 获取指定某一天内，按小时聚合的总使用时长。
        /// </summary>
        /// <returns>一个字典，Key是小时(0-23)，Value是该小时的总使用时长。</returns>
        Task<IDictionary<int, TimeSpan>> GetTotalHourlyUsageForDayAsync(DateOnly date, IEnumerable<Guid>? excludedProcessIds = null);

        /// <summary>
        /// 获取指定日期范围内，按天聚合的总使用时长。
        /// </summary>
        /// <returns>一个字典，Key是具体日期，Value是该日的总使用时长。</returns>
        Task<IDictionary<DateOnly, TimeSpan>> GetTotalDailyUsageForPeriodAsync(DateOnly startDate, DateOnly endDate, IEnumerable<Guid>? excludedProcessIds = null);

        /// <summary>
        /// 获取指定某一天内，单个进程在各个小时的使用时长分布。
        /// </summary>
        /// <returns>包含该进程在0-23点使用情况分布的数据。</returns>
        Task<IDictionary<int, TimeSpan>> GetProcessHourlyDistributionForDayAsync(DateOnly date, Guid processId);

        /// <summary>
        /// 获取指定日期范围内，单个进程在每一天的使用时长分布。
        /// </summary>
        /// <returns>包含该进程在日期范围内每日使用情况分布的数据。</returns>
        Task<IDictionary<DateOnly, TimeSpan>> GetProcessDailyDistributionForPeriodAsync(DateOnly startDate, DateOnly endDate, Guid processId);
    }
}