namespace ScreenTimeTracker.Infrastructure.Persistence.Models
{
    public class TrackerSettingsEntity
    {
        public required TimeSpan PollingInterval { get; set; }
        public required TimeSpan ProcessInfoStaleThreshold { get; set; }
        public required string ProcessIconDirPath { get; set; }
        public required bool EnableIdleDetection { get; set; }
        public required TimeSpan IdleTimeout { get; set; }
    }
}