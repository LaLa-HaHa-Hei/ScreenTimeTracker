using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using ScreenTimeTracker.ScreenTime.Features.Tracking;
using Tmds.DBus;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

/// <summary>
/// 定义 systemd-logind 的 D-Bus 接口映射
/// </summary>
[DBusInterface("org.freedesktop.login1.Manager")]
public interface ILoginManager : IDBusObject
{
    Task<IDisposable> WatchPrepareForSleepAsync(
        Action<bool> handler,
        Action<Exception>? onError = null
    );
    Task<IDisposable> WatchPrepareForShutdownAsync(
        Action<bool> handler,
        Action<Exception>? onError = null
    );
}

[SupportedOSPlatform("linux")]
public class LinuxSystemLifecycleProvider : ISystemLifecycleProvider, IDisposable
{
    public event EventHandler? Suspending;
    public event EventHandler? Resumed;
    public event EventHandler? ShuttingDown;

    private readonly List<IDisposable> _disposables = new();
    private bool _disposed;
    private bool _shuttingDownInvoked;

    public LinuxSystemLifecycleProvider()
    {
        // 1. 注册 POSIX 信号监听 (SIGTERM/SIGINT)，捕获 systemctl stop、kill 命令或关机时的进程终止信号

        _disposables.Add(
            PosixSignalRegistration.Create(PosixSignal.SIGTERM, _ => OnShuttingDown())
        );
        _disposables.Add(PosixSignalRegistration.Create(PosixSignal.SIGINT, _ => OnShuttingDown()));

        // 2. 异步初始化 D-Bus 监听 systemd-logind 广播的系统休眠与关机事件
        _ = InitializeDBusAsync();
    }

    private async Task InitializeDBusAsync()
    {
        try
        {
            // 连接 Linux 系统总线 (System Bus)
            var manager = Connection.System.CreateProxy<ILoginManager>(
                "org.freedesktop.login1",
                "/org/freedesktop/login1"
            );

            // 监听 PrepareForSleep 信号
            // goingToSleep = true 表示即将休眠；false 表示从休眠恢复
            var sleepSub = await manager.WatchPrepareForSleepAsync(goingToSleep =>
            {
                if (goingToSleep)
                    Suspending?.Invoke(this, EventArgs.Empty);
                else
                    Resumed?.Invoke(this, EventArgs.Empty);
            });

            // 监听 PrepareForShutdown 信号
            // goingToShutdown = true 表示系统正在发起关机/重启
            var shutdownSub = await manager.WatchPrepareForShutdownAsync(goingToShutdown =>
            {
                if (goingToShutdown)
                    OnShuttingDown();
            });

            lock (_disposables)
            {
                if (_disposed)
                {
                    sleepSub.Dispose();
                    shutdownSub.Dispose();
                }
                else
                {
                    _disposables.Add(sleepSub);
                    _disposables.Add(shutdownSub);
                }
            }
        }
        catch (Exception ex)
        {
            // 在无 D-Bus 或受限的 Linux 环境（如精简 Docker 容器）中容错
            System.Diagnostics.Debug.WriteLine(
                $"Failed to initialize D-Bus system lifecycle listener: {ex.Message}"
            );
        }
    }

    private void OnShuttingDown()
    {
        // 避免 D-Bus 信号与 SIGTERM 信号重复触发
        if (_shuttingDownInvoked)
            return;
        _shuttingDownInvoked = true;

        ShuttingDown?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        lock (_disposables)
        {
            if (_disposed)
                return;
            _disposed = true;

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();
        }

        GC.SuppressFinalize(this);
    }
}
