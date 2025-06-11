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
            Task.Run(() => MessageBox.Show("���������к󲻻���ʾ���棬ע�ⲻҪ�ظ����У�", "ע�⣡", MessageBoxButtons.OK, MessageBoxIcon.Information));
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
            MessageBox.Show($"{_trackerProcessName} ����ʧ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show($"{_webApiProcessName} ����ʧ��", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        using NotifyIcon trayIcon = new()
        {
            Icon = Properties.Resources.Icon,
            Visible = true,
            ContextMenuStrip = new()
        };

        trayIcon.ContextMenuStrip.Items.Add("��ʾ����", null, (_, _) =>
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {Shared.Constants.Web.BaseUrl}") { CreateNoWindow = true });
        });
        trayIcon.ContextMenuStrip.Items.Add("ֻ�˳�����", null, (_, _) =>
        {
            if (_isFirstRun)
            { 
                DialogResult result = MessageBox.Show($"�ò�����������̨��¼�ĳ�������кͺ����������������С��Ƿ�����˳���", "ע�⣡", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (result != DialogResult.OK) return;
            }
            Application.Exit();
        });
        trayIcon.ContextMenuStrip.Items.Add("�˳�����", null, (_, _) =>
        {
            if (_isFirstRun)
            {
                DialogResult result = MessageBox.Show($"�ò�����ɱ����Ϊ {_trackerProcessName} �� {_webApiProcessName} �Ľ��̡����������ɱ�����޸�Ŀ¼����������������Ӧexe�����֣����޸� {SettingsFileName} �ж�Ӧ�Ľ��������Ƿ�����˳���", "ע�⣡", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
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


        Application.Run(); // �޴���������Ϣѭ��
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
                MessageBox.Show($"��ֹ���� {processName} ʧ�ܡ�", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
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