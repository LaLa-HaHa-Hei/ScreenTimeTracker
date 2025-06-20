using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Tray;

static class Program
{
    private static Mutex? _mutex;
    private static readonly string _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "TraySettings.json");
    private static string _trackerAppPath = Path.Combine(AppContext.BaseDirectory, "Tracker.exe");
    private static string _webApiAppPath = Path.Combine(AppContext.BaseDirectory, "WebApi.exe");
    private static IntPtr _jobHandle;
    private static NotifyIcon? _trayIcon;
    private static bool _isExiting = false;

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

        ApplicationConfiguration.Initialize();

        // ���Ҫ�õĶ˿��Ƿ�ռ��
        if (IsPortInUse(Shared.Constants.Web.Port))
        {

            DialogResult result = MessageBox.Show($"�˿�{Shared.Constants.Web.Port}�ѱ�ռ�ã��벻Ҫ�ظ�������ر�ռ�ö˿ڵĳ����Ƿ�������У�", "����", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (result != DialogResult.Yes)
                return;
        }

        try
        {
            InitializeJobObject();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"��������{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Application.ApplicationExit += OnApplicationExit;

        InitializeSettings();

        // ��������һֱ���еĺ�̨�޴��ڳ���
        try
        {
            StartProcess(_trackerAppPath);
        }
        catch (Exception ex)
        {
            DialogResult result = MessageBox.Show($"{_trackerAppPath}����ʧ�ܣ��Ƿ�������У�����{ex.Message}", "����", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (result != DialogResult.Yes)
                return;
        }
        try
        {
            StartProcess(_webApiAppPath);
        }
        catch (Exception ex)
        {
            DialogResult result = MessageBox.Show($"{_webApiAppPath}����ʧ�ܣ��Ƿ�������У�����{ex.Message}", "����", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (result != DialogResult.Yes)
                return;
        }

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

    private static void InitializeJobObject()
    {
        _jobHandle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
        if (_jobHandle == IntPtr.Zero)
            throw new Exception("���� JobObject ʧ��");

        var info = new NativeTypes.JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            BasicLimitInformation = new NativeTypes.JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = NativeTypes.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            }
        };

        int length = Marshal.SizeOf(info);
        IntPtr infoPtr = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(info, infoPtr, false);

        bool success = NativeMethods.SetInformationJobObject(_jobHandle, NativeTypes.JobObjectExtendedLimitInformation, infoPtr, (uint)length);
        if (!success)
        {
            Marshal.FreeHGlobal(infoPtr);
            throw new Exception("���� JobObject ��Ϣʧ��");
        }
        Marshal.FreeHGlobal(infoPtr);
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

    private static Process StartProcess(string processPath)
    {
        // ����ļ��Ƿ����
        if (!File.Exists(processPath))
        {
            throw new FileNotFoundException($"�ļ�{processPath}������", processPath);
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
        });

        if (process != null)
        {
            if (!NativeMethods.AssignProcessToJobObject(_jobHandle, process.Handle))
                throw new Exception($"����{processPath}������󶨵� JobObject ʧ�ܣ����޷��汾����Ĺرն��Զ��ر�");
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                if (!_isExiting)
                    MessageBox.Show($"����{processPath}��δ֪ԭ��ر�", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            return process;
        }
        else
            throw new Exception("����δ֪����");
    }

    // �˳�ʱ��������������������
    private static void OnApplicationExit(object? sender, EventArgs e)
    {
        _isExiting = true;
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }
        if (_jobHandle != IntPtr.Zero)
        {
            NativeMethods.CloseHandle(_jobHandle);
            _jobHandle = IntPtr.Zero;
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
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                string jsonString = File.ReadAllText(_settingsFilePath);
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
            catch (Exception ex)
            {
                MessageBox.Show($"��ȡ�����ļ�ʧ�ܣ���ʹ�ò�д��Ĭ�����á�\n����{ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        try
        {
            File.WriteAllText(_settingsFilePath, defaultSettings);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"�޷�д��Ĭ������: {ex.Message}", "����", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}