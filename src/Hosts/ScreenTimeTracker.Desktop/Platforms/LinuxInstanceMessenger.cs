using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.Desktop.Hosting;

namespace ScreenTimeTracker.Desktop.Platforms;

[SupportedOSPlatform("linux")]
public partial class LinuxInstanceMessenger(ILogger<LinuxInstanceMessenger> logger)
    : IInstanceMessenger,
        IDisposable
{
    // Pipe 名字绑定路径 Hash
    private static readonly string PipeName = $"STT_{GetPathHash()}";
    private bool _disposed;
    private CancellationTokenSource? _cts;
    private Task? _listeningTask;

    public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

    public async Task<bool> SendMessageAsync(
        string message,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var clientStream = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            await clientStream.ConnectAsync(cancellationToken);
            byte[] msg = Encoding.UTF8.GetBytes(message);
            await clientStream.WriteAsync(msg.AsMemory(), cancellationToken);
            await clientStream.FlushAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            LogSendMessageFailed(logger, ex);
            return false;
        }
    }

    public Task StartListeningAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_listeningTask is not null && !_listeningTask.IsCompleted)
            return Task.CompletedTask;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _listeningTask = Task.Run(() => ListenLoopAsync(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }

    private async Task ListenLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var serverStream = new NamedPipeServerStream(
                    pipeName: PipeName,
                    direction: PipeDirection.In,
                    maxNumberOfServerInstances: 1,
                    transmissionMode: PipeTransmissionMode.Byte,
                    options: PipeOptions.Asynchronous
                );

                // 设置通用读写权限，允许其他用户（如单例检测到的第二进程）向该 Socket 发送唤醒消息
                string socketFilePath = Path.Combine(Path.GetTempPath(), $"CoreFxPipe_{PipeName}");
                if (File.Exists(socketFilePath))
                {
                    try
                    {
                        File.SetUnixFileMode(
                            socketFilePath,
                            UnixFileMode.UserRead
                                | UnixFileMode.UserWrite
                                | UnixFileMode.GroupRead
                                | UnixFileMode.GroupWrite
                                | UnixFileMode.OtherRead
                                | UnixFileMode.OtherWrite
                        );
                    }
                    catch { }
                }

                await serverStream.WaitForConnectionAsync(cancellationToken);

                using var reader = new StreamReader(serverStream, Encoding.UTF8, leaveOpen: true);
                string msg = await reader.ReadToEndAsync(cancellationToken);

                var handler = MessageReceived;
                if (handler is not null)
                {
                    _ = Task.Run(
                        () =>
                        {
                            try
                            {
                                handler(this, new MessageReceivedEventArgs(msg));
                            }
                            catch { }
                        },
                        CancellationToken.None
                    );
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                LogListenFailed(logger, ex);
                await Task.Delay(1000, cancellationToken);
            }
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

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to send message to instance.")]
    private static partial void LogSendMessageFailed(ILogger logger, Exception ex);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "An error occurred while listening for messages."
    )]
    private static partial void LogListenFailed(ILogger logger, Exception ex);

    public async Task StopListeningAsync(CancellationToken cancellationToken = default)
    {
        _cts?.Cancel();
        if (_listeningTask is not null)
        {
            try
            {
                await _listeningTask;
            }
            catch (OperationCanceledException) { }
            _listeningTask = null;
        }
        _cts?.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        if (_disposed)
            return;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
