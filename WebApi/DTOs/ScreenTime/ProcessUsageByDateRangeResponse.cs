namespace WebApi.DTOs.ScreenTime
{
    public class ProcessUsageByDateRangeResponse
    {
        public required List<ProcessUsage> ProcessUsages { get; set; }
        /// <summary>
        /// 某天内所有进程的总使用时间
        /// </summary>
        public required List<DailyUsageSummary> DailyUsageSummary { get; set; }
    }

    public class ProcessUsage
    {
        public required string ProcessName { get; set; }
        public long DurationMs { get; set; }
    }

    public class DailyUsageSummary
    {
        public required DateOnly Date { get; set; }
        /// <summary>
        /// 所有进程在该日的总使用时间（单位：毫秒）
        /// </summary>
        public long DurationMs { get; set; }
    }
}
