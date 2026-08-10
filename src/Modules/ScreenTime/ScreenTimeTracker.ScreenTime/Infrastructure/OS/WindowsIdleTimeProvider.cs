using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Features.Tracking;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("windows5.0")]
public partial class WindowsIdleTimeProvider(ILogger<WindowsIdleTimeProvider> logger)
    : IIdleTimeProvider
{
    private readonly ILogger<WindowsIdleTimeProvider> _logger = logger;

    public TimeSpan GetSystemIdleTime()
    {
        LASTINPUTINFO info = new();
        info.cbSize = (uint)Marshal.SizeOf(info);
        if (!PInvoke.GetLastInputInfo(ref info))
        {
            LogQueryIdleTimeFailed(_logger, Marshal.GetLastWin32Error());
            return TimeSpan.Zero;
        }

        uint tickCount = PInvoke.GetTickCount();
        uint idleTicks = tickCount - info.dwTime;
        return TimeSpan.FromMilliseconds(idleTicks);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "GetLastInputInfo failed with Win32 error code: {ErrorCode}. Assuming active status (TimeSpan.Zero)."
    )]
    private static partial void LogQueryIdleTimeFailed(ILogger logger, int errorCode);
}
