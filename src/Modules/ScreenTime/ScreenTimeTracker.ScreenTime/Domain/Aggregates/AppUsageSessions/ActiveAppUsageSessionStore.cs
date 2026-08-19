namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;

public class ActiveAppUsageSessionStore
{
    private ActiveAppUsageSessionState? _activeAppUsageSessionState;

    public ActiveAppUsageSessionState? Current
    {
        get => Volatile.Read(ref _activeAppUsageSessionState);
        set => Interlocked.Exchange(ref _activeAppUsageSessionState, value);
    }

    public ActiveAppUsageSessionState? Take() =>
        Interlocked.Exchange(ref _activeAppUsageSessionState, null);
}
