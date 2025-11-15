namespace ScreenTimeTracker.Application.Configuration
{
    public class TrackerOptions
    {
        public static readonly string SectionName = "TrackerSettings";
        public int PollingIntervalMilliseconds { get; set; }
        public double ProcessInfoStaleThresholdMinutes { get; set; }
        public required string ProcessIconDirPath { get; set; }
        public double IdleTimeoutMinutes { get; set; }
    }
}