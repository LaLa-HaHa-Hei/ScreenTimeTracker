namespace ScreenTimeTracker.ScreenTime.Domain.Apps;

public class ActiveAppUsageSessionStore
{
    public ActiveAppUsageSessionState? Current
    {
        get => Volatile.Read(ref field);
        set => Interlocked.Exchange(ref field, value);
    }
}
