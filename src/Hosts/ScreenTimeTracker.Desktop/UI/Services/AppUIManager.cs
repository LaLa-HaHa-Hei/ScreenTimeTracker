using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Photino.NET;
using ScreenTimeTracker.Desktop.Hosting;
using ScreenTimeTracker.Desktop.UI.State;
using ScreenTimeTracker.DesktopSettings.Contracts.Enums;

namespace ScreenTimeTracker.Desktop.UI.Services;

public interface IAppUIManager
{
    void OpenUI();
    void OpenUIInWindow();
    void OpenUIInBrowser();
}

public partial class AppUIManager : IAppUIManager
{
    private readonly ILogger<AppUIManager> _logger;
    private readonly IServerUrlProvider _urlProvider;
    private readonly IWindowPlacementStore _placementStore;
    private readonly IDesktopLocalSettingsProvider _desktopLocalSettingsProvider;
    private PhotinoWindow? _window;
    private readonly Lock _windowLock = new();

    public AppUIManager(
        ILogger<AppUIManager> logger,
        IServerUrlProvider urlProvider,
        IWindowPlacementStore placementStore,
        IHostApplicationLifetime lifetime,
        IDesktopLocalSettingsProvider desktopLocalSettingsProvider
    )
    {
        _logger = logger;
        _placementStore = placementStore;
        _urlProvider = urlProvider;
        _desktopLocalSettingsProvider = desktopLocalSettingsProvider;

        if (OperatingSystem.IsWindows())
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", "./Data/WebView2");
        else if (OperatingSystem.IsLinux())
        {
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", "./Data/WebKitGTK/data");
            Environment.SetEnvironmentVariable("XDG_CACHE_HOME", "./Data/WebKitGTK/cache");
        }

        lifetime.ApplicationStopping.Register(() =>
        {
            lock (_windowLock)
            {
                if (_window is not null)
                {
                    try
                    {
                        _window.Invoke(() => _window.Close());
                    }
                    catch { }
                }
            }
        });
    }

    public void OpenUI()
    {
        if (_desktopLocalSettingsProvider.DefaultUIOpenMode == UIOpenMode.Browser)
            OpenUIInBrowser();
        else
            OpenUIInWindow();
    }

    public void OpenUIInBrowser()
    {
        string uiUrl = _urlProvider.GetServerUrl();
        Process.Start(new ProcessStartInfo { FileName = uiUrl, UseShellExecute = true });
    }

    public void OpenUIInWindow()
    {
        string uiUrl = _urlProvider.GetServerUrl();

        lock (_windowLock)
        {
            if (_window is null)
            {
                ResetPhotinoMessageLoop();
                var uiThread = new Thread(() =>
                {
                    try
                    {
                        var placement = _placementStore.Load();

                        _window = new PhotinoWindow()
                            .SetTitle("Screen Time Tracker")
                            .SetUseOsDefaultSize(false)
                            .SetUseOsDefaultLocation(false)
                            .SetSize(new Size((int)placement.Width, (int)placement.Height));
                        if (double.IsNaN(placement.Left) || double.IsNaN(placement.Top))
                            _window.Center();
                        else
                        {
                            _window.SetLeft((int)placement.Left);
                            _window.SetTop((int)placement.Top);
                        }

                        var lastNormalPlacement = new WindowPlacement
                        {
                            Left = _window.Location.X,
                            Top = _window.Location.Y,
                            Width = _window.Size.Width,
                            Height = _window.Size.Height,
                        };

                        // 监听位置大小变化并保存
                        _window.RegisterLocationChangedHandler(
                            (sender, _) =>
                            {
                                if (
                                    sender is PhotinoWindow window
                                    && !window.Maximized
                                    && !window.Minimized
                                )
                                {
                                    lastNormalPlacement.Left = window.Location.X;
                                    lastNormalPlacement.Top = window.Location.Y;
                                }
                            }
                        );
                        _window.RegisterSizeChangedHandler(
                            (sender, _) =>
                            {
                                if (
                                    sender is PhotinoWindow window
                                    && !window.Maximized
                                    && !window.Minimized
                                )
                                {
                                    lastNormalPlacement.Width = window.Size.Width;
                                    lastNormalPlacement.Height = window.Size.Height;
                                }
                            }
                        );

                        _window.RegisterWindowClosingHandler(
                            (sender, _) =>
                            {
                                _placementStore.Save(lastNormalPlacement);
                                return false;
                            }
                        );

                        _window.Load(uiUrl);
                        _window.WaitForClose();
                    }
                    catch (Exception ex)
                    {
                        LogOpenUIInWindowFailed(_logger, ex);
                    }
                    finally
                    {
                        lock (_windowLock)
                            _window = null;
                    }
                })
                {
                    IsBackground = true,
                };
                if (OperatingSystem.IsWindows())
                    uiThread.SetApartmentState(ApartmentState.STA);

                uiThread.Start();
            }
            else
            {
                // 窗口已经存在，将其唤醒/带到前台
                _window.Invoke(() =>
                {
                    if (_window.Minimized)
                        _window.SetMinimized(false);
                    else
                    {
                        _window.SetTopMost(true);
                        _window.SetTopMost(false);
                    }
                });
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to open UI in window.")]
    private static partial void LogOpenUIInWindowFailed(ILogger logger, Exception ex);

    // https://github.com/tryphotino/photino.NET/issues/59
    // Photino.NET 的 bug 导致无法在 WaitForClose() 返回后创建第二个窗口
    private static void ResetPhotinoMessageLoop()
    {
        var field = typeof(PhotinoWindow).GetField(
            "_messageLoopIsStarted",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        field?.SetValue(null, false);
    }
}
