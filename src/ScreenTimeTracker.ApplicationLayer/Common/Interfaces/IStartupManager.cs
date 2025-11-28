namespace ScreenTimeTracker.ApplicationLayer.Common.Interfaces;

public interface IStartupManager
{
    public bool IsStartupEnabled(string appName);
    public void EnableStartup(string appName, string filePath);
    public void DisableStartup(string appName);
}
