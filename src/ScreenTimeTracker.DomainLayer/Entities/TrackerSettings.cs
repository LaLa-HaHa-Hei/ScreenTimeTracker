namespace ScreenTimeTracker.DomainLayer.Entities
{
    public class TrackerSettings(TimeSpan pollingInterval, TimeSpan processInfoStaleThreshold, string processIconDirPath, bool enableIdleDetection, TimeSpan idleTimeout)
    {
        public TimeSpan PollingInterval { get; set; } = pollingInterval;
        public TimeSpan ProcessInfoStaleThreshold { get; set; } = processInfoStaleThreshold;
        public string ProcessIconDirPath { get; set; } = processIconDirPath;
        public bool EnableIdleDetection { get; set; } = enableIdleDetection;
        public TimeSpan IdleTimeout { get; set; } = idleTimeout;

        public static TrackerSettings Default => new(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromHours(20),
            "./Data/Icons",
            true,
            TimeSpan.FromMinutes(10)
        );
    }
}