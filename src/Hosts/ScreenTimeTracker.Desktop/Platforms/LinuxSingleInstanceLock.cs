using System.IO;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.Desktop.Hosting;

namespace ScreenTimeTracker.Desktop.Platforms;

[SupportedOSPlatform("linux")]
public partial class LinuxSingleInstanceLock(ILogger<LinuxSingleInstanceLock> logger)
    : ISingleInstanceLock,
        IDisposable
{
    private static readonly string InstanceHash = GetPathHash();

    // 优先使用 XDG_RUNTIME_DIR（用户隔离），若不存在则使用 /tmp
    private static readonly string LockFilePath = Path.Combine(
        GetBaseTempDirectory(),
        $"screentimetracker_{InstanceHash}.lock"
    );

    private FileStream? _fileStream;
    private bool _disposed;

    public bool TryAcquire()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            var stream = new FileStream(
                LockFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.ReadWrite
            );

            // 设置 0666 权限，防止多用户运行同一个便携目录时，第二个用户无权读取 Lock 文件
            TrySetGlobalPermissions(LockFilePath);

            // 尝试加文件排他锁 (非阻塞)
            stream.Lock(0, long.MaxValue);

            _fileStream = stream;
            return true;
        }
        catch (IOException ex)
        {
            LogAlreadyRunning(logger, ex);
            CleanupStream();
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            LogAccessDenied(logger, ex);
            CleanupStream();
            return false;
        }
        catch (Exception ex)
        {
            LogFailedToAcquireLock(logger, ex);
            CleanupStream();
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

    private static string GetBaseTempDirectory()
    {
        string? xdgRuntime = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
        return !string.IsNullOrEmpty(xdgRuntime) && Directory.Exists(xdgRuntime)
            ? xdgRuntime
            : Path.GetTempPath();
    }

    private static void TrySetGlobalPermissions(string filePath)
    {
        try
        {
            File.SetUnixFileMode(
                filePath,
                UnixFileMode.UserRead
                    | UnixFileMode.UserWrite
                    | UnixFileMode.GroupRead
                    | UnixFileMode.GroupWrite
                    | UnixFileMode.OtherRead
                    | UnixFileMode.OtherWrite
            );
        }
        catch
        {
            // 忽略非文件所有者修改权限失败的情况
        }
    }

    private void CleanupStream()
    {
        _fileStream?.Dispose();
        _fileStream = null;
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Another instance for this directory is already running."
    )]
    private static partial void LogAlreadyRunning(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Access denied when opening lock file. Another user might own this lock."
    )]
    private static partial void LogAccessDenied(ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to acquire single instance lock.")]
    private static partial void LogFailedToAcquireLock(ILogger logger, Exception ex);

    public void Dispose()
    {
        if (_disposed)
            return;
        if (_fileStream != null)
        {
            try
            {
                _fileStream.Unlock(0, long.MaxValue);
            }
            catch { }
            _fileStream.Dispose();
            _fileStream = null;
        }
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
