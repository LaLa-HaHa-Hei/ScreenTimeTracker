namespace WebApi.DTOs.ScreenTime
{
    public class ProcessUsage
    {
        public required string ProcessName { get; set; }
        public long DurationMs { get; set; }
    }
}
