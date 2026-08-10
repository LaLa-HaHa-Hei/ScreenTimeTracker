using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Features.Tracking;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("linux")]
public partial class LinuxIdleTimeProvider(ILogger<LinuxIdleTimeProvider> logger)
    : IIdleTimeProvider
{
    private readonly ILogger<LinuxIdleTimeProvider> _logger = logger;

    public TimeSpan GetSystemIdleTime()
    {
        // 1. 优先尝试 X11 P/Invoke（适用于 Xorg 以及开启了 XWayland 的大多数桌面环境）
        if (TryGetX11IdleTime(out var x11IdleTime))
        {
            return x11IdleTime;
        }

        // 2. 如果在纯 Wayland (如 GNOME Wayland) 环境，尝试通过 DBus 查询
        if (TryGetGnomeWaylandIdleTime(out var gnomeIdleTime))
        {
            return gnomeIdleTime;
        }

        // 3. 兜底策略：尝试调用系统 xprintidle 命令行工具
        if (TryGetXPrintIdleTime(out var xprintIdleTime))
        {
            return xprintIdleTime;
        }

        LogQueryIdleTimeFailed(_logger);
        return TimeSpan.Zero;
    }

    private bool TryGetX11IdleTime(out TimeSpan idleTime)
    {
        idleTime = TimeSpan.Zero;

        try
        {
            IntPtr display = NativeX11.XOpenDisplay(null);
            if (display == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                IntPtr rootWindow = NativeX11.XDefaultRootWindow(display);
                IntPtr infoPtr = NativeX11.XScreenSaverAllocInfo();
                if (infoPtr == IntPtr.Zero)
                {
                    return false;
                }

                try
                {
                    if (NativeX11.XScreenSaverQueryInfo(display, rootWindow, infoPtr) != 0)
                    {
                        var info = Marshal.PtrToStructure<NativeX11.XScreenSaverInfo>(infoPtr);
                        ulong idleMs = info.idle.ToUInt64();
                        idleTime = TimeSpan.FromMilliseconds(idleMs);
                        return true;
                    }
                }
                finally
                {
                    // 修复警告：显式接收并使用 XFree 返回的错误码
                    int freeResult = NativeX11.XFree(infoPtr);
                    _ = freeResult;
                }
            }
            finally
            {
                _ = NativeX11.XCloseDisplay(display);
            }
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException)
        {
            // 静默忽略缺失 libX11.so 或 libXss.so 的情况，回退到 DBus 策略
        }
        catch (Exception ex)
        {
            LogX11QueryFailed(_logger, ex);
        }

        return false;
    }

    private static bool TryGetGnomeWaylandIdleTime(out TimeSpan idleTime)
    {
        idleTime = TimeSpan.Zero;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "gdbus",
                Arguments =
                    "call --session --dest org.gnome.Mutter.IdleMonitor --object-path /org/gnome/Mutter/IdleMonitor/Core --method org.gnome.Mutter.IdleMonitor.GetIdletime",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null)
                return false;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(300);

            // 输出格式示例: "(uint64 12345,)"
            if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
            {
                // 修复警告：显式指定非区域敏感的 StringComparison.Ordinal 比对策略
                int startIndex = output.IndexOf("uint64 ", StringComparison.Ordinal);
                if (startIndex != -1)
                {
                    startIndex += 7;
                    int endIndex = output.IndexOfAny([',', ')'], startIndex);
                    if (endIndex != -1)
                    {
                        string numStr = output[startIndex..endIndex].Trim();
                        if (ulong.TryParse(numStr, out ulong idleMs))
                        {
                            idleTime = TimeSpan.FromMilliseconds(idleMs);
                            return true;
                        }
                    }
                }
            }
        }
        catch
        {
            // 忽略未安装 gdbus 或非 GNOME 环境的异常
        }

        return false;
    }

    private static bool TryGetXPrintIdleTime(out TimeSpan idleTime)
    {
        idleTime = TimeSpan.Zero;

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "xprintidle",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null)
                return false;

            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(300);

            if (process.ExitCode == 0 && ulong.TryParse(output.Trim(), out ulong idleMs))
            {
                idleTime = TimeSpan.FromMilliseconds(idleMs);
                return true;
            }
        }
        catch
        {
            // 忽略未安装 xprintidle 的异常
        }

        return false;
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Error occurred while querying X11 idle time."
    )]
    private static partial void LogX11QueryFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to query system idle time across all Linux strategies (X11/DBus/xprintidle). Assuming active status (TimeSpan.Zero)."
    )]
    private static partial void LogQueryIdleTimeFailed(ILogger logger);

    private static class NativeX11
    {
        private const string LibX11 = "libX11.so.6";
        private const string LibXss = "libXss.so.1";

        [DllImport(
            LibX11,
            CharSet = CharSet.Ansi,
            BestFitMapping = false,
            ThrowOnUnmappableChar = true
        )]
        public static extern IntPtr XOpenDisplay(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? displayName
        );

        [DllImport(LibX11)]
        public static extern IntPtr XDefaultRootWindow(IntPtr display);

        [DllImport(LibX11)]
        public static extern int XCloseDisplay(IntPtr display);

        [DllImport(LibX11)]
        public static extern int XFree(IntPtr ptr);

        [DllImport(LibXss)]
        public static extern IntPtr XScreenSaverAllocInfo();

        [DllImport(LibXss)]
        public static extern int XScreenSaverQueryInfo(
            IntPtr display,
            IntPtr drawable,
            IntPtr saverInfo
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct XScreenSaverInfo
        {
            public IntPtr window;
            public int state;
            public int kind;
            public UIntPtr til_or_since;
            public UIntPtr idle; // milliseconds
            public UIntPtr eventMask;
        }
    }
}
