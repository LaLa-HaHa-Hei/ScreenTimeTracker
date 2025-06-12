namespace WebApi.DTOs.ScreenTime
{
    public class ProcessUsageByDateRangeResponse
    {
        public required List<ProcessUsage> ProcessUsages { get; set; }
        /// <summary>
        /// 某天内所有进程的总使用时间
        /// </summary>
        public required List<DateUsage> DailyUsageSummary { get; set; }
    }
}
