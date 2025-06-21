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
                MessageBox.Show("�����Ѿ��������С�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();

            // ���Ҫ�õĶ˿��Ƿ�ռ��
            if (IsPortInUse(Shared.Constants.Web.Port))
            {
                DialogResult result = MessageBox.Show($"�˿�{Shared.Constants.Web.Port}�ѱ�ռ�ã��벻Ҫ�ظ�������ر�ռ�ö˿ڵĳ����Ƿ�������У�", "����", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (result != DialogResult.Yes)
                    return;
            }

            _tracker = new TrackerHost();
            await _tracker.StartAsync();
            _webApi = new WebApiHost();
            await _webApi.StartAsync();

            InitializeTrayIcon();

            Application.Run(new ApplicationContext()); // �޴���������Ϣѭ��
        }

        private static void InitializeTrayIcon()
        {
            _trayIcon = new()
            {
                Icon = Properties.Resources.Icon ?? SystemIcons.Application,
                Visible = true,
                ContextMenuStrip = new(),
                Text = "��Ļʹ��ʱ��"
            };

            _trayIcon.ContextMenuStrip.Items.Add("��ʾ����", null, (_, _) =>
            {
                Process.Start(new ProcessStartInfo { FileName = Shared.Constants.Web.BaseUrl, UseShellExecute = true });
            });
            _trayIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            _trayIcon.ContextMenuStrip.Items.Add("�˳�", null, (_, _) =>
            {
                Application.Exit();
            });

            // ����ʱ��ҳ��
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

        // �˳�ʱ��������������������
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
            // ��ȡ���л�Ծ��TCP����
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpListeners = properties.GetActiveTcpListeners();

            // ���˿��Ƿ���ʹ����
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