namespace Data.Entities
{
    /// <summary>
    /// 某天中 x 到 x+1 小时内某程序的使用时间
    /// 采用复合主键：ProcessName, Date, Hour
    /// </summary>
    public class HourlyUsage
    {
        public required string ProcessName { get; set; }
        public DateOnly Date { get; set; }
        public int Hour { get; set; } // 0-23
        public long DurationMs { get; set; }
    }
}
