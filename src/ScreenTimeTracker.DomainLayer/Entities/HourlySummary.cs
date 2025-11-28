namespace ScreenTimeTracker.DomainLayer.Entities
{
    public class HourlySummary(ProcessInfo trackedProcess, DateTime hour, TimeSpan totalDuration)
    {
        public ProcessInfo TrackedProcess { get; init; } = trackedProcess;
        public DateTime Hour { get; init; } = hour;
        public TimeSpan TotalDuration { get; private set; } = totalDuration;

        public static HourlySummary Reconstitute(ProcessInfo trackedProcess, DateTime hour, TimeSpan totalDuration)
        {
            return new HourlySummary(trackedProcess, hour, totalDuration);
        }

        public void AddDuration(TimeSpan duration)
        {
            TotalDuration += duration;
        }
    }
}
