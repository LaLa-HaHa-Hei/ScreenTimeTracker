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
            MessageBox.Show("�����Ѿ��������С�", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // ���Ҫ�õĶ˿��Ƿ�ռ��
        if (IsPortInUse(Shared.Constants.Web.Port))
        {

            MessageBox.Show($"�˿�{Shared.Constants.Web.Port}�ѱ�ռ�ã��벻Ҫ�ظ�������ر�ռ�ö˿ڵĳ���", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.ApplicationExit += OnApplicationExit;

        InitializeSettings();

        // ����������̨����
        if (!StartProcess(_trackerAppPath))
            return;
        if (!StartProcess(_webApiAppPath))
        {
            OnApplicationExit(null, EventArgs.Empty); // ������������ tracker
            return;
        }

        using NotifyIcon trayIcon = new()
        {
            Icon = Properties.Resources.Icon,
            Visible = true,
            ContextMenuStrip = new(),
            Text = "��Ļʹ��ʱ��"
        };

        trayIcon.ContextMenuStrip.Items.Add("��ʾ����", null, (_, _) =>
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Shared.Constants.Web.BaseUrl}") { CreateNoWindow = true });
        });
        trayIcon.ContextMenuStrip.Items.Add("�˳�", null, (_, _) =>
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

        Application.Run(); // �޴���������Ϣѭ��
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
            MessageBox.Show($"�������� {processPath} ʱ����: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        return false;
    }

    // �˳�ʱ��������������������
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
                MessageBox.Show($"��ֹ���� {process.ProcessName} ����: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

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