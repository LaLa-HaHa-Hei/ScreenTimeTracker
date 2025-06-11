using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Tracker.Services.Interfaces;

namespace Tracker.Services
{
    internal class ForegroundWindowService(ILogger<ForegroundWindowService> logger) : IForegroundWindowService
    {
        private readonly ILogger<ForegroundWindowService> _logger = logger;
        public Process? GetForegroundWindowProcess()
        {
            IntPtr hwnd = NativeMethods.GetForegroundWindow();
            _ = NativeMethods.GetWindowThreadProcessId(hwnd, out int pid);
            try
            {
                return Process.GetProcessById(pid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get process by ID **{ProcessId}**", pid);
                return null;
            }
        }
    }
}
