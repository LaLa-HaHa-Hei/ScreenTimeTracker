namespace ScreenTimeTracker.Domain.Entities
{
    public class ActivityInterval(Guid id, DateTime timestamp, ProcessInfo trackedProcess, TimeSpan duration)
    {
        public Guid Id { get; init; } = id;
        public DateTime Timestamp { get; private set; } = timestamp;
        public ProcessInfo TrackedProcess { get; private set; } = trackedProcess;
        public TimeSpan Duration { get; private set; } = duration;

        public static ActivityInterval Reconstitute(Guid id, DateTime timestamp, ProcessInfo trackedProcess, TimeSpan duration)
        {
            return new ActivityInterval(id, timestamp, trackedProcess, duration);
        }

        public void UpdateProcess(ProcessInfo processInfo)
        {
            TrackedProcess = processInfo;
        }
    }
}
