namespace ScreenTimeTracker.DomainLayer.Entities
{
    public class UserConfiguration(TrackerSettings trackerSettings, AggregationSettings aggregationSettings)
    {
        public TrackerSettings Tracker { get; set; } = trackerSettings;
        public AggregationSettings Aggregation { get; set; } = aggregationSettings;

        public static UserConfiguration Default => new(
            TrackerSettings.Default,
            AggregationSettings.Default
        );
    }
}