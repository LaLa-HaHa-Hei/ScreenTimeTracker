namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;

public class ActiveAppUsageSessionStore
{
    public ActiveAppUsageSessionState? Current
    {
        get => Volatile.Read(ref field);
        set => Interlocked.Exchange(ref field, value);
    }
}
