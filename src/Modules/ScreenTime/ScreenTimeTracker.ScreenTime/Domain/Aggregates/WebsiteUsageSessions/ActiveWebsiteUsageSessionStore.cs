namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;

public class ActiveWebsiteUsageSessionStore
{
    public ActiveWebsiteUsageSessionState? Current
    {
        get => Volatile.Read(ref field);
        set => Interlocked.Exchange(ref field, value);
    }
}
