namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public interface IIdleTimeProvider
{
    TimeSpan GetSystemIdleTime();
}
