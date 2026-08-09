using System.Runtime.Versioning;
using Microsoft.Win32;
using ScreenTimeTracker.ScreenTime.Features.Tracking;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("windows")]
public class WindowsSystemLifecycleProvider : ISystemLifecycleProvider, IDisposable
{
    public event EventHandler? Suspending;
    public event EventHandler? Resumed;
    public event EventHandler? ShuttingDown;

    public WindowsSystemLifecycleProvider()
    {
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
        SystemEvents.SessionEnding += OnSessionEnding;
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Suspend)
            Suspending?.Invoke(this, EventArgs.Empty);
        else if (e.Mode == PowerModes.Resume)
            Resumed?.Invoke(this, EventArgs.Empty);
    }

    private void OnSessionEnding(object sender, SessionEndingEventArgs e)
    {
        ShuttingDown?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
        SystemEvents.SessionEnding -= OnSessionEnding;
        GC.SuppressFinalize(this);
    }
}
