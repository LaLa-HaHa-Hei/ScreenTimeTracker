namespace ScreenTimeTracker.ScreenTime.Domain;

public interface IActiveAppUsageSessionStore
{
    ActiveAppUsageSessionState? Current { get; set; }
}
