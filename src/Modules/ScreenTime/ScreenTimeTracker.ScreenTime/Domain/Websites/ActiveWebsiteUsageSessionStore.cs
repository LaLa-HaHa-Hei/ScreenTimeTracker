namespace ScreenTimeTracker.ScreenTime.Domain.Websites;

public class ActiveWebsiteUsageSessionStore
{
    public ActiveWebsiteUsageSessionState? Current
    {
        get => Volatile.Read(ref field);
        set => Interlocked.Exchange(ref field, value);
    }
}
