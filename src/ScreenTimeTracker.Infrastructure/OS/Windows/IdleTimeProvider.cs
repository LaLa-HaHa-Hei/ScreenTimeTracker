using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer.Common.Interfaces;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace ScreenTimeTracker.Infrastructure.OS.Windows;

#pragma warning disable CA1416 // Validate platform compatibility
public class IdleTimeProvider(ILogger<IdleTimeProvider> logger) : IIdleTimeProvider
{
    private readonly ILogger<IdleTimeProvider> _logger = logger;

    public Task<TimeSpan> GetSystemIdleTimeAsync()
    {
        LASTINPUTINFO info = new();
        info.cbSize = (uint)Marshal.SizeOf(info);
        if (!PInvoke.GetLastInputInfo(ref info))
        {
            int errorCode = Marshal.GetLastWin32Error();
            _logger.LogWarning("GetLastInputInfo failed with Win32 error code: {ErrorCode}. Assuming active status (TimeSpan.Zero).", errorCode);
            return Task.FromResult(TimeSpan.Zero);
        }

        uint tickCount = PInvoke.GetTickCount();
        uint idleTicks = tickCount - info.dwTime;
        return Task.FromResult(TimeSpan.FromMilliseconds(idleTicks));
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
