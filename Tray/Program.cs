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

        try
        {
            InitializeJobObject();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"发生错误：{ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        Application.ApplicationExit += OnApplicationExit;

        InitializeSettings();

        // 启动两个一直运行的后台无窗口程序
        try
        {
            StartProcess(_trackerAppPath);
        }
        catch (Exception ex)
        {
            DialogResult result = MessageBox.Show($"{_trackerAppPath}启动失败，是否继续运行？错误：{ex.Message}", "错误", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (result != DialogResult.Yes)
                return;
        }
        try
        {
            StartProcess(_webApiAppPath);
        }
        catch (Exception ex)
        {
            DialogResult result = MessageBox.Show($"{_webApiAppPath}启动失败，是否继续运行？错误：{ex.Message}", "错误", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            if (result != DialogResult.Yes)
                return;
        }

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

    private static void InitializeJobObject()
    {
        _jobHandle = NativeMethods.CreateJobObject(IntPtr.Zero, null);
        if (_jobHandle == IntPtr.Zero)
            throw new Exception("创建 JobObject 失败");

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
            throw new Exception("设置 JobObject 信息失败");
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
        // 检查文件是否存在
        if (!File.Exists(processPath))
        {
            throw new FileNotFoundException($"文件{processPath}不存在", processPath);
        }

        var process = Process.Start(new ProcessStartInfo
        {
            FileName = processPath,
            UseShellExecute = false,
        });

        if (process != null)
        {
            if (!NativeMethods.AssignProcessToJobObject(_jobHandle, process.Handle))
                throw new Exception($"进程{processPath}启动后绑定到 JobObject 失败，将无法随本程序的关闭而自动关闭");
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) =>
            {
                if (!_isExiting)
                    MessageBox.Show($"进程{processPath}因未知原因关闭", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            return process;
        }
        else
            throw new Exception("发生未知错误");
    }

    // 退出时结束掉启动的两个程序
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
                MessageBox.Show($"读取配置文件失败，将使用并写入默认配置。\n错误：{ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            MessageBox.Show($"无法写入默认配置: {ex.Message}", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}