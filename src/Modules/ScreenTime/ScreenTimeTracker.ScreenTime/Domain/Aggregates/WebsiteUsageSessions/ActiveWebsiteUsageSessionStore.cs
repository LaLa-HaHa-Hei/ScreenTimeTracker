namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;

public class ActiveWebsiteUsageSessionStore
{
    private ActiveWebsiteUsageSessionState? _activeWebsiteUsageSessionState;
    public ActiveWebsiteUsageSessionState? Current
    {
        get => Volatile.Read(ref _activeWebsiteUsageSessionState);
        set => Interlocked.Exchange(ref _activeWebsiteUsageSessionState, value);
    }

    public ActiveWebsiteUsageSessionState? Take() =>
        Interlocked.Exchange(ref _activeWebsiteUsageSessionState, null);
}
