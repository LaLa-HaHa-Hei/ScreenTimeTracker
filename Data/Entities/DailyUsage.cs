namespace Data.Entities
{
    /// <summary>
    /// 某天中某个程序的使用时间汇总
    /// </summary>
    public class DailyUsage
    {
        public required string ProcessName { get; set; }
        public DateOnly Date { get; set; }
        public long DurationMs { get; set; }
    }
}
