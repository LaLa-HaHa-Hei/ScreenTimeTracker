using System.Diagnostics;
using System.Drawing;
using System.Runtime.Versioning;
using H.NotifyIcon.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.Desktop.UI.Services;
using ScreenTimeTracker.Desktop.UI.State;
using Windows.Win32;

namespace ScreenTimeTracker.Desktop.Platforms;

[SupportedOSPlatform("windows5.1.2600")]
public partial class TrayService : ITrayService, IDisposable
{
    private readonly ILogger<TrayService> _logger;
    private readonly IAppUIManager _appUIManager;
    private readonly IDesktopLocalSettingsProvider _desktopLocalSettingsProvider;
    private readonly IStringLocalizer<TrayService> _localizer;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly PopupMenuItem _openAppDirItem;
    private readonly PopupMenuItem _openUIBrowserItem;
    private readonly PopupMenuItem _openUIWindowItem;
    private readonly PopupMenuItem _exitItem;

    private readonly Icon? _iconHandle;
    private nint _fallbackIconHandle = nint.Zero;
    private readonly TrayIconWithContextMenu _trayIcon;
    private bool _shouldBeVisible;
    private bool _isVisible;
    private bool _disposed;

    public TrayService(
        ILogger<TrayService> logger,
        IAppUIManager appUIManager,
        IDesktopLocalSettingsProvider desktopLocalSettingsProvider,
        IStringLocalizer<TrayService> localizer,
        IHostApplicationLifetime lifetime
    )
    {
        _logger = logger;
        _appUIManager = appUIManager;
        _desktopLocalSettingsProvider = desktopLocalSettingsProvider;
        _localizer = localizer;
        _lifetime = lifetime;

        using var iconStream = typeof(Program).Assembly.GetManifestResourceStream("AppIcon");
        if (iconStream is not null)
            _iconHandle = new Icon(iconStream);
        else
        {
            LogIconNotFoundWarning(_logger);
            _fallbackIconHandle = PInvoke.LoadIcon(lpIconName: PInvoke.IDI_APPLICATION);
            _iconHandle = null;
        }

        _openAppDirItem = new PopupMenuItem(string.Empty, (_, _) => OpenAppDirectory());
        _openUIBrowserItem = new PopupMenuItem(
            string.Empty,
            (_, _) => _appUIManager.OpenUIInBrowser()
        );
        _openUIWindowItem = new PopupMenuItem(
            string.Empty,
            (_, _) => _appUIManager.OpenUIInWindow()
        );
        _exitItem = new PopupMenuItem(string.Empty, (_, _) => ExitApplication());

        _trayIcon = new TrayIconWithContextMenu
        {
            Icon = _iconHandle is null ? _fallbackIconHandle : _iconHandle.Handle,
            ToolTip = "Screen Time Tracker",
            ContextMenu = new PopupMenu
            {
                Items =
                {
                    _openAppDirItem,
                    new PopupMenuSeparator(),
                    _openUIBrowserItem,
                    _openUIWindowItem,
                    new PopupMenuSeparator(),
                    _exitItem,
                },
            },
        };

        ApplyLanguage();

        _trayIcon.MessageWindow.TaskbarCreated += OnTaskbarCreated;

        _desktopLocalSettingsProvider.OnSettingChanged += HandleAppSettingsChanged;

        _trayIcon.MessageWindow.MouseEventReceived += (_, e) =>
        {
            if (e.MouseEvent != MouseEvent.IconLeftMouseUp)
                return;
            _appUIManager.OpenUI();
        };
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Embedded resource 'AppIcon' not found. Creating a fallback solid color icon."
    )]
    private static partial void LogIconNotFoundWarning(ILogger logger);

    public void Show()
    {
        if (_shouldBeVisible)
            return;
        _shouldBeVisible = true;
        if (_isVisible)
            return;

        if (!_trayIcon.IsCreated)
            _ = EnsureTrayIconCreatedAsync();
        else
        {
            _trayIcon.Show();
            _isVisible = true;
        }
    }

    public void Hide()
    {
        if (!_shouldBeVisible)
            return;

        _shouldBeVisible = false;
        if (_trayIcon.IsCreated && _isVisible)
        {
            _trayIcon.Hide();
            _isVisible = false;
        }
    }

    private async Task EnsureTrayIconCreatedAsync(int maxAttempts = 15, int delayMs = 1000)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (!_shouldBeVisible)
                return;

            if (_trayIcon.IsCreated)
            {
                _isVisible = true;
                return;
            }

            if (CanCreateTrayIcon())
            {
                _trayIcon.Create();
                _isVisible = true;
            }

            await Task.Delay(delayMs);
        }

        LogTrayIconCreationFailed(_logger, maxAttempts);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to create tray icon after {MaxAttempts} attempts. Taskbar was not ready."
    )]
    private static partial void LogTrayIconCreationFailed(ILogger logger, int maxAttempts);

    private bool CanCreateTrayIcon()
    {
        try
        {
            using var testIcon = new TrayIcon
            {
                Icon = _iconHandle is null ? _fallbackIconHandle : _iconHandle.Handle,
            };
            testIcon.Create();
            testIcon.TryRemove();
            return true;
        }
        catch (Exception ex)
        {
            LogTestIconCreationFailed(_logger, ex);
            return false;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Test icon creation failed. Taskbar is not ready yet."
    )]
    private static partial void LogTestIconCreationFailed(ILogger logger, Exception ex);

    private void OnTaskbarCreated(object? sender, EventArgs e)
    {
        LogTaskbarCreated(_logger);

        if (_trayIcon.IsCreated)
        {
            _trayIcon.TryRemove();
        }
        if (_shouldBeVisible)
        {
            _trayIcon.Create();
            _isVisible = true;
        }
        else
            _isVisible = false;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Taskbar created.")]
    private static partial void LogTaskbarCreated(ILogger logger);

    private void HandleAppSettingsChanged(object? sender, SettingChangedEventArgs e)
    {
        if (e.SettingName == nameof(IDesktopLocalSettingsProvider.Language))
            ApplyLanguage();
    }

    private void ApplyLanguage()
    {
        var culture = new System.Globalization.CultureInfo(_desktopLocalSettingsProvider.Language);
        System.Globalization.CultureInfo.CurrentUICulture = culture;

        _openAppDirItem.Text = _localizer["OpenAppDirectory"];
        _openUIBrowserItem.Text = _localizer["OpenUIInBrowser"];
        _openUIWindowItem.Text = _localizer["OpenUIInWindow"];
        _exitItem.Text = _localizer["Exit"];
    }

    private static void OpenAppDirectory()
    {
        Process.Start(
            new ProcessStartInfo { FileName = AppContext.BaseDirectory, UseShellExecute = true }
        );
    }

    private void ExitApplication()
    {
        _lifetime.StopApplication();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _desktopLocalSettingsProvider.OnSettingChanged -= HandleAppSettingsChanged;
            _trayIcon.Dispose();
            _iconHandle?.Dispose();
        }

        // 使用系统共享图标，不销毁
        // if (_fallbackIconHandle != nint.Zero)
        // {
        //     PInvoke.DestroyIcon(new HICON(_fallbackIconHandle));
        //     _fallbackIconHandle = nint.Zero;
        // }

        _disposed = true;
    }
}
