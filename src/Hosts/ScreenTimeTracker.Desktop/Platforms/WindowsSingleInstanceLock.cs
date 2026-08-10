using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.Desktop.Hosting;

namespace ScreenTimeTracker.Desktop.Platforms;

[SupportedOSPlatform("windows")]
public partial class WindowsSingleInstanceLock(ILogger<WindowsSingleInstanceLock> logger)
    : ISingleInstanceLock,
        IDisposable
{
    // 将 Global\ 前缀与路径哈希结合，既支持跨用户/管理员权限，又能隔离不同目录的便携版
    private static readonly string MutexName = $@"Global\STT_Mutex_{GetPathHash()}";

    private Mutex? _mutex;
    private bool _disposed;

    public bool TryAcquire()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var mutexSecurity = new MutexSecurity();
            mutexSecurity.AddAccessRule(
                new MutexAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    MutexRights.FullControl,
                    AccessControlType.Allow
                )
            );

            // initiallyOwned: true -> 尝试在创建的同时原子性地直接获取初始所有权
            var localMutex = MutexAcl.Create(
                initiallyOwned: true,
                name: MutexName,
                out bool createdNew,
                mutexSecurity
            );

            if (createdNew)
            {
                // 创建成功并获得了所有权
                _mutex = localMutex;
                return true;
            }

            // 如果 Mutex 已经存在，尝试非阻塞 WaitOne 检查是否被丢弃（前一进程崩溃）
            try
            {
                if (localMutex.WaitOne(0))
                {
                    _mutex = localMutex;
                    return true;
                }
            }
            catch (AbandonedMutexException ex)
            {
                LogAbandonedMutexAcquired(logger, ex);
                _mutex = localMutex;
                return true;
            }

            // 未能获取所有权，释放本地句柄引用
            localMutex.Dispose();
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            LogMutexAccessDenied(logger, ex);
            return false;
        }
        catch (Exception ex)
        {
            LogFailedToAcquireMutex(logger, ex);
            return false;
        }
    }

    private static string GetPathHash()
    {
        string appDir = Path.GetFullPath(AppContext.BaseDirectory)
            .TrimEnd(Path.DirectorySeparatorChar)
            .ToLowerInvariant();
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(appDir));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Previous instance crashed. Acquired abandoned mutex."
    )]
    private static partial void LogAbandonedMutexAcquired(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Access denied when creating mutex. Likely another instance is running with higher privileges."
    )]
    private static partial void LogMutexAccessDenied(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occurred when trying to acquire the mutex."
    )]
    private static partial void LogFailedToAcquireMutex(ILogger logger, Exception ex);

    public void Dispose()
    {
        if (_disposed)
            return;

        // Mutex 具有线程亲和性（Thread Affinity）。在 DI 容器异步关闭时，
        // Dispose 可能在非 WaitOne/Create 的 TaskPool 线程执行，调用 ReleaseMutex 会抛出 ApplicationException。
        // 直接 Dispose 释放 OS 句柄，Windows 内核会在句柄关闭或进程退出时自动安全释放 Mutex。
        _mutex?.Dispose();
        _mutex = null;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
