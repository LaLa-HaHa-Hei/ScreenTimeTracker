using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsageSession;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("windows5.0")]
public partial class WindowsIdleTimeProvider(ILogger<WindowsIdleTimeProvider> logger)
    : IIdleTimeProvider
{
    private readonly ILogger<WindowsIdleTimeProvider> _logger = logger;

    public Task<TimeSpan> GetSystemIdleTimeAsync()
    {
        LASTINPUTINFO info = new();
        info.cbSize = (uint)Marshal.SizeOf(info);
        if (!PInvoke.GetLastInputInfo(ref info))
        {
            LogQueryIdleTimeFailed(_logger, Marshal.GetLastWin32Error());
            return Task.FromResult(TimeSpan.Zero);
        }

        uint tickCount = PInvoke.GetTickCount();
        uint idleTicks = tickCount - info.dwTime;
        return Task.FromResult(TimeSpan.FromMilliseconds(idleTicks));
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "GetLastInputInfo failed with Win32 error code: {errorCode}. Assuming active status (TimeSpan.Zero)."
    )]
    private static partial void LogQueryIdleTimeFailed(ILogger logger, int errorCode);
}
