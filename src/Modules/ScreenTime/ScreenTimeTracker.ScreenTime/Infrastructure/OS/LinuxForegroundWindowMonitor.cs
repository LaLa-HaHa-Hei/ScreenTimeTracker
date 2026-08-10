using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("linux")]
public partial class LinuxForegroundWindowMonitor : IForegroundWindowMonitor, IDisposable
{
    public event EventHandler<WindowInfo?>? ForegroundWindowChanged;

    private readonly ILogger<LinuxForegroundWindowMonitor> _logger;
    private readonly CancellationTokenSource _cts = new();
    private readonly Thread _monitorThread;
    private bool _disposed;

    public LinuxForegroundWindowMonitor(ILogger<LinuxForegroundWindowMonitor> logger)
    {
        _logger = logger;

        _monitorThread = new Thread(ListenForWindowChanges)
        {
            IsBackground = true,
            Name = "LinuxForegroundWindowMonitorThread",
        };
        _monitorThread.Start();
    }

    public WindowInfo? GetForegroundWindow()
    {
        IntPtr display = X11.XOpenDisplay(null);
        if (display == IntPtr.Zero)
            return null;

        try
        {
            IntPtr rootWindow = X11.XDefaultRootWindow(display);
            return GetActiveWindowInfo(display, rootWindow);
        }
        finally
        {
            _ = X11.XCloseDisplay(display); // 使用弃元忽略 XCloseDisplay 返回值
        }
    }

    private void ListenForWindowChanges()
    {
        IntPtr display = X11.XOpenDisplay(null);
        if (display == IntPtr.Zero)
        {
            LogFailedToOpenDisplay(_logger);
            return;
        }

        try
        {
            IntPtr rootWindow = X11.XDefaultRootWindow(display);
            IntPtr netActiveWindowAtom = X11.XInternAtom(display, "_NET_ACTIVE_WINDOW", false);

            // 1. 检查 XSelectInput 的返回值以解决静态代码分析警告
            int selectResult = X11.XSelectInput(display, rootWindow, X11.PropertyChangeMask);
            if (selectResult == 0)
            {
                LogFailedToSelectInput(_logger);
                return;
            }

            IntPtr lastActiveWindow = IntPtr.Zero;

            while (!_cts.Token.IsCancellationRequested)
            {
                if (X11.XPending(display) > 0)
                {
                    // 2. 使用弃元运算符接收 XNextEvent 返回值
                    _ = X11.XNextEvent(display, out X11.XEvent ev);

                    if (ev.type == X11.PropertyNotify && ev.xproperty.atom == netActiveWindowAtom)
                    {
                        IntPtr activeWindow = GetActiveWindowId(display, rootWindow);
                        if (activeWindow != lastActiveWindow)
                        {
                            lastActiveWindow = activeWindow;
                            WindowInfo? info = GetWindowInfoByWindowId(display, activeWindow);
                            ForegroundWindowChanged?.Invoke(this, info);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }
        catch (Exception ex)
        {
            LogForegroundWindowThreadCrashed(_logger, ex);
        }
        finally
        {
            _ = X11.XCloseDisplay(display);
        }
    }

    private static WindowInfo? GetActiveWindowInfo(IntPtr display, IntPtr rootWindow)
    {
        IntPtr activeWindow = GetActiveWindowId(display, rootWindow);
        return GetWindowInfoByWindowId(display, activeWindow);
    }

    private static IntPtr GetActiveWindowId(IntPtr display, IntPtr rootWindow)
    {
        IntPtr activeWindowAtom = X11.XInternAtom(display, "_NET_ACTIVE_WINDOW", false);
        if (
            GetWindowProperty(
                display,
                rootWindow,
                activeWindowAtom,
                X11.XA_WINDOW,
                out IntPtr[]? data
            )
            && data?.Length > 0
        )
        {
            return data[0];
        }
        return IntPtr.Zero;
    }

    private static WindowInfo? GetWindowInfoByWindowId(IntPtr display, IntPtr windowId)
    {
        if (windowId == IntPtr.Zero)
            return null;

        IntPtr pidAtom = X11.XInternAtom(display, "_NET_WM_PID", false);
        if (
            !GetWindowProperty(display, windowId, pidAtom, X11.XA_CARDINAL, out IntPtr[]? data)
            || data == null
            || data.Length == 0
        )
        {
            return null;
        }

        int pid = (int)data[0];
        if (pid <= 0)
            return null;

        return GetWindowInfoFromPid(pid);
    }

    private static WindowInfo? GetWindowInfoFromPid(int pid)
    {
        try
        {
            string? executablePath = null;
            string procExeLink = $"/proc/{pid}/exe";

            if (File.Exists(procExeLink))
            {
                var target = File.ResolveLinkTarget(procExeLink, returnFinalTarget: true);
                executablePath = target?.FullName;
            }

            string? processName = executablePath is not null
                ? Path.GetFileNameWithoutExtension(executablePath)
                : null;

            if (processName is null)
            {
                using var process = Process.GetProcessById(pid);
                processName = process.ProcessName;
                executablePath ??= process.MainModule?.FileName;
            }

            return new WindowInfo(processName, executablePath);
        }
        catch
        {
            return null;
        }
    }

    private static bool GetWindowProperty(
        IntPtr display,
        IntPtr window,
        IntPtr property,
        IntPtr reqType,
        out IntPtr[]? data
    )
    {
        data = null;
        int status = X11.XGetWindowProperty(
            display,
            window,
            property,
            IntPtr.Zero,
            (IntPtr)1,
            false,
            reqType,
            out _,
            out int actualFormat,
            out IntPtr nitems,
            out _,
            out IntPtr propData
        );

        if (status != 0 || propData == IntPtr.Zero || nitems == IntPtr.Zero)
            return false;

        try
        {
            int count = (int)nitems;
            data = new IntPtr[count];
            if (actualFormat == 32)
            {
                for (int i = 0; i < count; i++)
                {
                    data[i] = Marshal.ReadIntPtr(propData, i * IntPtr.Size);
                }
            }
            return true;
        }
        finally
        {
            _ = X11.XFree(propData); // 3. 使用弃元忽略 XFree 返回值
        }
    }

    [LoggerMessage(Level = LogLevel.Critical, Message = "ForegroundWindowMonitor thread crashed.")]
    private static partial void LogForegroundWindowThreadCrashed(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to open X11 Display connection. Foreground window monitoring disabled."
    )]
    private static partial void LogFailedToOpenDisplay(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to set X11 input mask for active window monitoring."
    )]
    private static partial void LogFailedToSelectInput(ILogger logger);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        _disposed = true;

        if (disposing)
        {
            _cts.Cancel();
            _cts.Dispose();
            ForegroundWindowChanged = null;
        }
    }

    #region X11 Native P/Invoke Bindings

    private static class X11
    {
        private const string LibX11 = "libX11.so.6";

        public const int PropertyNotify = 28;
        public const nint PropertyChangeMask = 1 << 22;
        public static readonly nint XA_CARDINAL = 6;
        public static readonly nint XA_WINDOW = 33;

        [DllImport(
            LibX11,
            CharSet = CharSet.Ansi,
            BestFitMapping = false,
            ThrowOnUnmappableChar = true
        )]
        public static extern IntPtr XOpenDisplay(
            [MarshalAs(UnmanagedType.LPUTF8Str)] string? display_name
        );

        [DllImport(LibX11)]
        public static extern int XCloseDisplay(IntPtr display);

        [DllImport(LibX11)]
        public static extern IntPtr XDefaultRootWindow(IntPtr display);

        [DllImport(
            LibX11,
            CharSet = CharSet.Ansi,
            BestFitMapping = false,
            ThrowOnUnmappableChar = true
        )]
        public static extern IntPtr XInternAtom(
            IntPtr display,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string atom_name,
            bool only_if_exists
        );

        [DllImport(LibX11)]
        public static extern int XSelectInput(IntPtr display, IntPtr window, IntPtr event_mask);

        [DllImport(LibX11)]
        public static extern int XPending(IntPtr display);

        [DllImport(LibX11)]
        public static extern int XNextEvent(IntPtr display, out XEvent event_return);

        [DllImport(LibX11)]
        public static extern int XGetWindowProperty(
            IntPtr display,
            IntPtr window,
            IntPtr property,
            IntPtr long_offset,
            IntPtr long_length,
            bool delete,
            IntPtr req_type,
            out IntPtr actual_type_return,
            out int actual_format_return,
            out IntPtr nitems_return,
            out IntPtr bytes_after_return,
            out IntPtr prop_return
        );

        [DllImport(LibX11)]
        public static extern int XFree(IntPtr data);

        [StructLayout(LayoutKind.Explicit, Size = 192)]
        public struct XEvent
        {
            [FieldOffset(0)]
            public int type;

            [FieldOffset(0)]
            public XPropertyEvent xproperty;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XPropertyEvent
        {
            public int type;
            public UIntPtr serial;
            public bool send_event;
            public IntPtr display;
            public IntPtr window;
            public IntPtr atom;
            public IntPtr time;
            public int state;
        }
    }

    #endregion
}
