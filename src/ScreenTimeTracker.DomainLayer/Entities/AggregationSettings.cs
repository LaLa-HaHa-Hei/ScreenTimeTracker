namespace ScreenTimeTracker.DomainLayer.Entities
{
    public class AggregationSettings(TimeSpan pollingInterval)
    {
        public TimeSpan PollingInterval { get; set; } = pollingInterval;

        public static AggregationSettings Default => new(TimeSpan.FromMinutes(60));
    }
}