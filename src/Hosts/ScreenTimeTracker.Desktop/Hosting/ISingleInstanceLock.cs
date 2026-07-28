namespace ScreenTimeTracker.Desktop.Hosting;

public interface ISingleInstanceLock
{
    bool TryAcquire();
}
