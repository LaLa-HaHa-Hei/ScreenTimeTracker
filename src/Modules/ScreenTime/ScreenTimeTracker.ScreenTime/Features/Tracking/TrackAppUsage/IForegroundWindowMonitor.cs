namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public interface IForegroundWindowMonitor
{
    WindowInfo? GetForegroundWindow();
    event EventHandler<WindowInfo?> ForegroundWindowChanged;
}
