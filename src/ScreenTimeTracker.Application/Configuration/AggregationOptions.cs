namespace ScreenTimeTracker.Application.Configuration
{
    public class AggregationOptions
    {
        public static readonly string SectionName = "AggregationSettings";
        public double PollingIntervalMinutes { get; set; }
    }
}