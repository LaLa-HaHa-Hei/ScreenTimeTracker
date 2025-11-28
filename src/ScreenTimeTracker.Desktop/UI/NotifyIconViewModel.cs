using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer.Common.Interfaces;
using ScreenTimeTracker.Desktop.Services;

namespace ScreenTimeTracker.Desktop.UI;

public partial class NotifyIconViewModel(ILogger<NotifyIconViewModel> logger, IStartupManager startupManager, AppRuntimeData runtimeData) : ObservableObject
{
    private const string AppName = "ScreenTimeTracker";
    private readonly ILogger<NotifyIconViewModel> _logger = logger;
    private readonly IStartupManager _startupManager = startupManager;
    private readonly AppRuntimeData _runtimeData = runtimeData;

    [ObservableProperty]
    private bool _isStartupEnabled = startupManager.IsStartupEnabled(AppName);


    /// <summary>
    /// Toggles the startup setting for the application.
    /// </summary>
    [RelayCommand]
    public void ToggleStartup()
    {
        bool isEnabled = _startupManager.IsStartupEnabled(AppName);
        if (isEnabled)
        {
            _startupManager.DisableStartup(AppName);
        }
        else
        {
            MessageBox.Show("该功能会修改系统配置，删除程序前请务必关闭，否则将导致残留！", "警告！", MessageBoxButton.OK, MessageBoxImage.Warning);
            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exePath))
            {
                _logger.LogError("Failed to get current executable path. Cannot enable startup.");
                MessageBox.Show("无法获取程序路径", "错误！", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _startupManager.EnableStartup(AppName, exePath);
        }

        bool current = _startupManager.IsStartupEnabled(AppName);
        IsStartupEnabled = current;
        if (isEnabled == current)
            MessageBox.Show("切换失败，可能因为权限不足，请以管理员身份运行", "错误！", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// Shows a window, if none is already open.
    /// </summary>
    [RelayCommand]
    public static void ShowWindow()
    {
        if (Application.Current.MainWindow is null)
            Application.Current.MainWindow = new MainWindow();

        Application.Current.MainWindow.Show();
    }

    /// <summary>
    /// Hides the main window.
    /// </summary>
    [RelayCommand]
    public static void HideWindow()
    {
        Application.Current.MainWindow?.Hide();
    }

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    [RelayCommand]
    public static void ExitApplication()
    {
        Application.Current.Shutdown();
    }

    [RelayCommand]
    public void OpenWebUI()
    {
        string? url = _runtimeData.BaseUrl;
        if (string.IsNullOrEmpty(url))
        {
            _logger.LogError("BaseUrl is null");
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
