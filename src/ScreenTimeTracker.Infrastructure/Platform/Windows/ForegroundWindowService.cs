using ScreenTimeTracker.Application.Interfaces;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ScreenTimeTracker.Infrastructure.Platform.Windows
{
#pragma warning disable CA1416 // Validate platform compatibility
    public class ForegroundWindowService : IForegroundWindowService
    {
        public Task<Process?> GetForegroundProcessAsync()
        {
            HWND hwnd = PInvoke.GetForegroundWindow();
            if (hwnd == HWND.Null)
                return Task.FromResult<Process?>(null);
            uint processId;
            unsafe
            {
                _ = PInvoke.GetWindowThreadProcessId(hwnd, &processId);
            }
            if (processId == 0)
                return Task.FromResult<Process?>(null);

            Process process = Process.GetProcessById((int)processId);

            return Task.FromResult<Process?>(process);
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility
}
