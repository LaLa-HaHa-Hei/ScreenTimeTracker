namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public interface IIdleTimeProvider
{
    Task<TimeSpan> GetSystemIdleTimeAsync();
}
