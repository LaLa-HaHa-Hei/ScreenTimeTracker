using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using NotificationIcon.NET;
using ScreenTimeTracker.Desktop.UI.Services;
using ScreenTimeTracker.Desktop.UI.State;

namespace ScreenTimeTracker.Desktop.Platforms;

public partial class LinuxTrayService : ITrayService, IDisposable
{
    private static readonly string _iconRelativePath = "./Resources/Icon.png";
    private static readonly string _iconPath = Path.Combine(
        AppContext.BaseDirectory,
        _iconRelativePath
    );

    private readonly ILogger<LinuxTrayService> _logger;
    private readonly IAppUIManager _appUIManager;
    private readonly IDesktopLocalSettingsProvider _desktopLocalSettingsProvider;
    private readonly IStringLocalizer<LinuxTrayService> _localizer;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly MenuItem _openAppDirItem;
    private readonly MenuItem _openUIBrowserItem;
    private readonly MenuItem _openUIWindowItem;
    private readonly MenuItem _exitItem;

    private NotifyIcon? _trayIcon;
    private bool _disposed;

    public LinuxTrayService(
        ILogger<LinuxTrayService> logger,
        IAppUIManager appUIManager,
        IDesktopLocalSettingsProvider desktopLocalSettingsProvider,
        IStringLocalizer<LinuxTrayService> localizer,
        IHostApplicationLifetime lifetime
    )
    {
        _logger = logger;
        _appUIManager = appUIManager;
        _desktopLocalSettingsProvider = desktopLocalSettingsProvider;
        _localizer = localizer;
        _lifetime = lifetime;

        _openAppDirItem = new(string.Empty) { Click = (s, e) => OpenAppDirectory() };
        _openUIBrowserItem = new(string.Empty)
        {
            Click = (s, e) => _appUIManager.OpenUIInBrowser(),
        };
        _openUIWindowItem = new(string.Empty) { Click = (s, e) => _appUIManager.OpenUIInWindow() };
        _exitItem = new(string.Empty) { Click = (s, e) => ExitApplication() };

        _trayIcon = NotifyIcon.Create(
            _iconPath,
            [_openAppDirItem, _openUIBrowserItem, _openUIWindowItem, _exitItem]
        );

        ApplyLanguage();

        _desktopLocalSettingsProvider.OnSettingChanged += HandleAppSettingsChanged;
    }

    public void Show()
    {
        _trayIcon ??= CreateTrayIcon();
        _trayIcon.Show();
    }

    public void Hide()
    {
        _trayIcon?.Dispose();
        _trayIcon = null;
    }

    private NotifyIcon CreateTrayIcon()
    {
        if (!File.Exists(_iconPath))
            LogIconNotFoundWarning(_logger, _iconPath);

        return NotifyIcon.Create(
            _iconPath,
            [_openAppDirItem, _openUIBrowserItem, _openUIWindowItem, _exitItem]
        );
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Icon file not found at path: {IconPath}")]
    private static partial void LogIconNotFoundWarning(ILogger logger, string iconPath);

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
            _trayIcon?.Dispose();
        }

        _disposed = true;
    }
}
