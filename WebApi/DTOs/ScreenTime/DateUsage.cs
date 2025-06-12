namespace WebApi.DTOs.ScreenTime
{
    public class DateUsage
    {
        public required DateOnly Date { get; set; }
        public long DurationMs { get; set; }
    }
}
