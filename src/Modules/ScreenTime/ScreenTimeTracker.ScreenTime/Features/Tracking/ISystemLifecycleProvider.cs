namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public interface ISystemLifecycleProvider
{
    event EventHandler? Suspending;
    event EventHandler? Resumed;
    event EventHandler? ShuttingDown;
}
