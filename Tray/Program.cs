using System.Diagnostics;
using System.Text.Json;

namespace Tray;

static class Program
{
    private static readonly string SettingsFileName = "TraySettings.json";
    private static string _trackerProcessName = "Tracker";
    private static string _webApiProcessName = "WebApi";
    private static bool _isFirstRun = false;

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        if (!File.Exists("FirstRun.txt"))
        {
            _isFirstRun = true;
            Task.Run(() => MessageBox.Show("本程序运行后不会显示界面，注意不要重复运行！", "注意！", MessageBoxButtons.OK, MessageBoxIcon.Information));
            File.Create("FirstRun.txt").Dispose();
        }
        if (File.Exists(SettingsFileName))
        {
            string jsonString = File.ReadAllText(SettingsFileName);
            using JsonDocument doc = JsonDocument.Parse(jsonString);
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("TrackerProcessName", out var trackerProp) &&
                root.TryGetProperty("WebApiProcessName", out var webApiProp))
            {
                string? trackerProcessName = trackerProp.GetString();
                string? webApiProcessName = webApiProp.GetString();

                if (!string.IsNullOrWhiteSpace(trackerProcessName) &&
                    !string.IsNullOrWhiteSpace(webApiProcessName))
                {
                    _trackerProcessName = trackerProcessName;
                    _webApiProcessName = webApiProcessName;
                }
                else
                    WriteDefaultSettings();
            }
            else
                WriteDefaultSettings();
        }
        else
            WriteDefaultSettings();


        var trackerProcess = new ProcessStartInfo
        {
            FileName = _trackerProcessName + ".exe",
            UseShellExecute = false,
        };
        try
        {
            Process.Start(trackerProcess);
        }
        catch
        {
            MessageBox.Show($"{_trackerProcessName} 启动失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        var webApiProcess = new ProcessStartInfo
        {
            FileName = _webApiProcessName + ".exe",
            UseShellExecute = false,
        };
        try
        {
            Process.Start(webApiProcess);
        }
        catch
        {
            MessageBox.Show($"{_webApiProcessName} 启动失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        using NotifyIcon trayIcon = new()
        {
            Icon = Properties.Resources.Icon,
            Visible = true,
            ContextMenuStrip = new()
        };

        trayIcon.ContextMenuStrip.Items.Add("显示界面", null, (_, _) =>
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Shared.Constants.Web.BaseUrl}") { CreateNoWindow = true });
        });
        trayIcon.ContextMenuStrip.Items.Add("只退出托盘", null, (_, _) =>
        {
            if (_isFirstRun)
            { 
                DialogResult result = MessageBox.Show($"该操作将保留后台记录的程序的运行和后天服务器程序的运行。是否继续退出？", "注意！", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK) return;
            }
            Application.Exit();
        });
        trayIcon.ContextMenuStrip.Items.Add("退出所有", null, (_, _) =>
        {
            if (_isFirstRun)
            {
                DialogResult result = MessageBox.Show($"该操作将杀死名为 {_trackerProcessName} 和 {_webApiProcessName} 的进程。如果发现误杀，请修改目录下这两个进程名对应exe的名字，并修改 {SettingsFileName} 中对应的进程名。是否继续退出？", "注意！", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK) return;
            }
            KillProcessByName(_trackerProcessName);
            KillProcessByName(_webApiProcessName);
            Application.Exit();
        });

        trayIcon.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {Shared.Constants.Web.BaseUrl}") { CreateNoWindow = true });
            }
        };


        Application.Run(); // 无窗体运行消息循环
    }
    private static void KillProcessByName(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);
        foreach (var process in processes)
        {
            try
            {
                process.Kill();
            }
            catch
            {
                MessageBox.Show($"终止进程 {processName} 失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
    private static void WriteDefaultSettings()
    {
        string defaultSettings = @"{
    ""TrackerProcessName"": ""Tracker"",
    ""WebApiProcessName"": ""WebApi""
}";
        File.WriteAllText(SettingsFileName, defaultSettings);
    }
}