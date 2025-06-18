using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace Tray;

static class Program
{
    private static Mutex? _mutex;
    private static readonly string _settingsFileName = "TraySettings.json";
    private static string _trackerAppPath = Path.Combine(AppContext.BaseDirectory, "Tracker.exe");
    private static string _webApiAppPath = Path.Combine(AppContext.BaseDirectory, "WebApi.exe");
    private static readonly List<Process> _startedProcesses = [];

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        if (!EnsureSingleInstance())
        {
            MessageBox.Show("程序已经在运行中。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // 检查要用的端口是否被占用
        if (IsPortInUse(Shared.Constants.Web.Port))
        {

            MessageBox.Show($"端口{Shared.Constants.Web.Port}已被占用，请不要重复启动或关闭占用端口的程序！", "错误！", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.ApplicationExit += OnApplicationExit;

        InitializeSettings();

        // 启动两个后台程序
        if (!StartProcess(_trackerAppPath))
            return;
        if (!StartProcess(_webApiAppPath))
        {
            OnApplicationExit(null, EventArgs.Empty); // 清理已启动的 tracker
            return;
        }

        using NotifyIcon trayIcon = new()
        {
            Icon = Properties.Resources.Icon,
            Visible = true,
            ContextMenuStrip = new(),
            Text = "屏幕使用时间"
        };

        trayIcon.ContextMenuStrip.Items.Add("显示界面", null, (_, _) =>
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Shared.Constants.Web.BaseUrl}") { CreateNoWindow = true });
        });
        trayIcon.ContextMenuStrip.Items.Add("退出", null, (_, _) =>
        {
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

    private static bool EnsureSingleInstance()
    {
        _mutex = new Mutex(true, "TrayAppMutexName", out bool createdNew);
        if (!createdNew)
        {
            return false;
        }
        return true;
    }

    private static bool StartProcess(string processPath)
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
        };

        try
        {
            var process = Process.Start(processInfo);
            if (process != null)
            {
                _startedProcesses.Add(process);
                return true;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"启动进程 {processPath} 时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return false;
    }

    // 退出时结束掉启动的两个程序
    private static void OnApplicationExit(object? sender, EventArgs e)
    {
        foreach (var process in _startedProcesses)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"终止进程 {process.ProcessName} 出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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

    private static void InitializeSettings()
    {
        if (File.Exists(_settingsFileName))
        {
            try
            {
                string jsonString = File.ReadAllText(_settingsFileName);
                using JsonDocument doc = JsonDocument.Parse(jsonString);
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("TrackerAppPath", out var trackerProp) &&
                    root.TryGetProperty("WebApiAppPath", out var webApiProp))
                {
                    string? trackerProcessName = trackerProp.GetString();
                    string? webApiProcessName = webApiProp.GetString();

                    if (!string.IsNullOrWhiteSpace(trackerProcessName) &&
                        !string.IsNullOrWhiteSpace(webApiProcessName))
                    {
                        _trackerAppPath = Path.Combine(AppContext.BaseDirectory, trackerProcessName);
                        _webApiAppPath = Path.Combine(AppContext.BaseDirectory, webApiProcessName);
                    }
                    else
                        WriteDefaultSettings();
                }
                else
                    WriteDefaultSettings();
            }
            catch
            {
                WriteDefaultSettings();
            }
        }
        else
            WriteDefaultSettings();
    }

    private static void WriteDefaultSettings()
    {
        string defaultSettings = @"{
    ""TrackerAppPath"": ""Tracker.exe"",
    ""WebApiAppPath"": ""WebApi.exe""
}";
        File.WriteAllText(_settingsFileName, defaultSettings);
    }
}