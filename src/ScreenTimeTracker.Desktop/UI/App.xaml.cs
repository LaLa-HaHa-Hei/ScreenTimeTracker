using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ScreenTimeTracker.Desktop.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? notifyIcon;
    public IServiceProvider? ServiceProvider { get; set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

        if (ServiceProvider != null)
        {
            var viewModel = ServiceProvider.GetRequiredService<NotifyIconViewModel>();
            notifyIcon.DataContext = viewModel;
        }

        // notifyIcon.ForceCreate();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        notifyIcon?.Dispose();
        base.OnExit(e);
    }
}

