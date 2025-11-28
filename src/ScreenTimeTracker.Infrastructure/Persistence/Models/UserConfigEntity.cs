namespace ScreenTimeTracker.Infrastructure.Persistence.Models
{
    public class UserConfigEntity
    {
        public required TrackerSettingsEntity Tracker { get; set; }
        public required AggregationSettingsEntity Aggregation { get; set; }
    }
}