using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using H.NotifyIcon.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ScreenTimeTracker.Tray
{
#pragma warning disable CA1416 // 验证平台兼容性
    public class Program()
    {
        private const string AppName = "ScreenTimeTracker";
        private static ILogger<Program> _logger = null!;
        private static Icon _icon = null!;
        private static TrayIconWithContextMenu _trayIcon = null!;
        private static PopupMenuItem _startUpMenuItem = null!;
        private static ConfigurationManager _configuration = null!;

        public static void Main(string[] args)
        {

            Mutex _mutex = new(true, "ScreenTimeTrackerTrayUniqueMutexName", out bool createdNew);
            if (!createdNew)
            {
                PInvoke.MessageBox(default, "程序已经启动，请查看托盘处", "注意！", MESSAGEBOX_STYLE.MB_OK);
                return;
            }

            // 切换工作目录为程序所在目录
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration
                .AddJsonFile("tray.appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"tray.appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

            _configuration = builder.Configuration;

            // 添加日志并配置
            builder.Services.AddSerilog((services, loggerConfig) =>
            {
                loggerConfig.ReadFrom.Configuration(_configuration);
            });

            var host = builder.Build();

            _logger = host.Services.GetRequiredService<ILogger<Program>>();

            InitializeTrayIcon();
            StartWebApiAndWorkerService();

            if (_configuration.GetValue("OpenUIOnStartup", true))
                OpenUI();

            _logger.LogInformation("Starting Tray host.");
            host.Run();
        }

        private static void StartWebApiAndWorkerService()
        {
            string? filePath;
            filePath = _configuration["WorkerServiceExecutablePath"];
            if (filePath is null)
                _logger.LogError("WorkerServiceExecutablePath is null.");
            else
                StartChildProcess(filePath, "WorkerService");
            filePath = _configuration["WebApiExecutablePath"];
            if (filePath is null)
                _logger.LogError("WebApiExecutablePath is null.");
            else
                StartChildProcess(filePath, "WebApi");
        }

        private static void StartChildProcess(string filename, string name)
        {
            if (!File.Exists(filename))
            {
                _logger.LogError("File {ExePath} not found.", filename);
                return;
            }
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = false,
            });
            if (process is null)
            {
                _logger.LogError("Failed to start process {Name} at {ExePath}", name, filename);
                return;
            }
            try
            {
                ChildProcessManager.AddProcess(process);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add process {Name} to job object.", name);
            }
        }

        private static void OpenUI()
        {
            string? url = _configuration["DefaultUIUrl"];
            if (url is null)
            {
                _logger.LogError("DefaultUIUrl is null");
                return;
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private static void InitializeTrayIcon()
        {
            using var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ScreenTimeTracker.Tray.Resources.Icon.ico");
            if (iconStream is null)
            {
                _logger.LogError("Failed to load tray icon.");
                return;
            }
            _icon = new Icon(iconStream);
            _trayIcon = new TrayIconWithContextMenu()
            {
                Icon = _icon.Handle,
                ToolTip = "Screen Time Tracker",
            };

            _trayIcon.MessageWindow.MouseEventReceived += (sender, e) =>
            {
                if (e.MouseEvent == MouseEvent.IconLeftMouseUp)
                {
                    OpenUI();
                }
            };

            _startUpMenuItem = new PopupMenuItem("开启自启动", (_, _) => ToogleStartup())
            {
                Checked = StartupManager.IsStartupEnabled(AppName)
            };
            _trayIcon.ContextMenu = new PopupMenu
            {
                Items =
                {
                    new PopupMenuItem("打开程序目录", (_, _) => {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = AppContext.BaseDirectory,
                            UseShellExecute = true
                        });
                    }),
                    _startUpMenuItem,
                    new PopupMenuSeparator(),
                    new PopupMenuItem("显示", (_, _) => OpenUI()),
                    new PopupMenuItem("退出", (_, _) =>
                    {
                        _trayIcon.Dispose();
                        Environment.Exit(0);
                    }),
                },
            };
            _trayIcon.Create();
        }

        private static void ToogleStartup()
        {
            bool isEnabled = StartupManager.IsStartupEnabled(AppName);
            if (isEnabled)
            {
                StartupManager.DisableStartup(AppName);
            }
            else
            {
                PInvoke.MessageBox(default, "该功能会修改系统配置，删除程序前请务必关闭，否则将导致残留！", "警告！", MESSAGEBOX_STYLE.MB_OK | MESSAGEBOX_STYLE.MB_ICONWARNING);
                string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                if (string.IsNullOrEmpty(exePath))
                {
                    _logger.LogError("Failed to get current executable path. Cannot enable startup.");
                    PInvoke.MessageBox(default, "无法获取程序路径", "错误！", MESSAGEBOX_STYLE.MB_OK);
                    return;
                }

                StartupManager.EnableStartup(AppName, exePath);
            }

            bool current = StartupManager.IsStartupEnabled(AppName);
            _startUpMenuItem.Checked = current;
            if (isEnabled == current)
                PInvoke.MessageBox(default, "切换失败，可能因为权限不足，请以管理员身份运行", "错误！", MESSAGEBOX_STYLE.MB_OK);
        }
    }
#pragma warning restore CA1416 // 验证平台兼容性
}