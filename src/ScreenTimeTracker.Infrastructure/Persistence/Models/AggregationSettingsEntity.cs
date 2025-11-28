namespace ScreenTimeTracker.Infrastructure.Persistence.Models
{
    public class AggregationSettingsEntity
    {
        public required TimeSpan PollingInterval { get; set; }
    }
}