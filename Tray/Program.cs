using System.Diagnostics;
using System.Net.NetworkInformation;
using Tracker;
using WebApi;

namespace Tray
{
    static class Program
    {
        private static Mutex? _mutex;
        private static NotifyIcon? _trayIcon;
        private static TrackerHost? _tracker;
        private static WebApiHost? _webApi;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            if (!EnsureSingleInstance())
            {
                MessageBox.Show("程序已经在运行中。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();

            // 检查要用的端口是否被占用
            if (IsPortInUse(Shared.Constants.Web.Port))
            {
                DialogResult result = MessageBox.Show($"端口{Shared.Constants.Web.Port}已被占用，请不要重复启动或关闭占用端口的程序！是否继续运行？", "错误！", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result != DialogResult.Yes)
                    return;
            }

            _tracker = new TrackerHost();
            await _tracker.StartAsync();
            _webApi = new WebApiHost();
            await _webApi.StartAsync();

            InitializeTrayIcon();

            Application.Run(new ApplicationContext()); // 无窗体运行消息循环
        }

        private static void InitializeTrayIcon()
        {
            _trayIcon = new()
            {
                Icon = Properties.Resources.Icon ?? SystemIcons.Application,
                Visible = true,
                ContextMenuStrip = new(),
                Text = "屏幕使用时间"
            };

            _trayIcon.ContextMenuStrip.Items.Add("显示界面", null, (_, _) =>
            {
                Process.Start(new ProcessStartInfo { FileName = Shared.Constants.Web.BaseUrl, UseShellExecute = true });
            });
            _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _trayIcon.ContextMenuStrip.Items.Add("退出", null, (_, _) =>
            {
                Application.Exit();
            });

            // 单击时打开页面
            _trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Process.Start(new ProcessStartInfo { FileName = Shared.Constants.Web.BaseUrl, UseShellExecute = true });
                }
            };
        }

        private static bool EnsureSingleInstance()
        {
            _mutex = new Mutex(true, "ScreenTimeTrayUniqueMutexName", out bool createdNew);
            if (!createdNew)
            {
                return false;
            }
            return true;
        }

        // 退出时结束掉启动的两个程序
        private static async Task OnApplicationExitAsync(object? sender, EventArgs e)
        {
            if (_tracker != null)
                await _tracker.StopAsync();
            if (_webApi != null)
                await _webApi.StopAsync();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }

        private static bool IsPortInUse(int port)
        {
            // 获取所有活跃的TCP连接
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = properties.GetActiveTcpListeners();

            // 检查端口是否在使用中
            foreach (var listener in tcpListeners)
            {
                if (listener.Port == port)
                {
                    return true;
                }
            }
            return false;
        }
    }
}