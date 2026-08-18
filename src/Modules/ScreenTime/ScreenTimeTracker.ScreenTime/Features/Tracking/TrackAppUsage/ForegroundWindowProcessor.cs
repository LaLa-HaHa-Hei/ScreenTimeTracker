using System.Threading.Channels;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Apps;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.UserSettings;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public record ForegroundWindowChangedMessage(WindowInfo? WindowInfo, DateTimeOffset OccurredAt);

public partial class ForegroundWindowProcessor(
    ILogger<ForegroundWindowProcessor> logger,
    IServiceScopeFactory serviceScopeFactory,
    UserIdleStore userIdleStore,
    SystemSuspendStore systemSuspendStore
) : BackgroundService
{
    private readonly Channel<ForegroundWindowChangedMessage> _channel =
        Channel.CreateUnbounded<ForegroundWindowChangedMessage>(
            new UnboundedChannelOptions { SingleReader = true }
        );

    private DateTimeOffset _lastProcessedAt = DateTimeOffset.MinValue;

    public void Enqueue(ForegroundWindowChangedMessage message)
    {
        _channel.Writer.TryWrite(message);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                // 处于挂起/空闲状态时，丢弃事件
                if (
                    userIdleStore.Current.IsUserIdle
                    || systemSuspendStore.Current.IsSystemSuspendActive
                )
                    continue;

                // 丢弃比已处理时间还早的过期/倒序事件
                if (message.OccurredAt <= _lastProcessedAt)
                    continue;

                try
                {
                    await ProcessWindowChangeAsync(message, stoppingToken);
                    _lastProcessedAt = message.OccurredAt;
                }
                catch (Exception ex)
                {
                    LogProcessWindowChangeFailed(
                        logger,
                        message.WindowInfo?.ProcessName ?? App.UnknownAppProcessName,
                        ex
                    );
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to process foreground change event for process '{ProcessName}'."
    )]
    private static partial void LogProcessWindowChangeFailed(
        ILogger logger,
        string processName,
        Exception ex
    );

    private async Task ProcessWindowChangeAsync(
        ForegroundWindowChangedMessage message,
        CancellationToken cancellationToken
    )
    {
        using var scope = serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        var executableMetadataProvider =
            scope.ServiceProvider.GetRequiredService<IExecutableMetadataProvider>();
        var activeSessionStore =
            scope.ServiceProvider.GetRequiredService<ActiveAppUsageSessionStore>();

        Guid appId;
        if (message.WindowInfo is null)
            appId = App.UnknownAppId;
        else
            appId = await IdentifyApp(
                context,
                executableMetadataProvider,
                message.OccurredAt,
                message.WindowInfo,
                cancellationToken
            );

        if (activeSessionStore.Current is null)
        {
            activeSessionStore.Current = new ActiveAppUsageSessionState(appId, message.OccurredAt);
        }
        else if (activeSessionStore.Current.AppId != appId)
        {
            await context.PersistActiveSessionAsync(
                activeSessionStore.Current,
                message.OccurredAt,
                cancellationToken
            );
            activeSessionStore.Current = new ActiveAppUsageSessionState(appId, message.OccurredAt);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public static async Task<Guid> IdentifyApp(
        ScreenTimeDbContext context,
        IExecutableMetadataProvider executableMetadataProvider,
        DateTimeOffset occurredAt,
        WindowInfo windowInfo,
        CancellationToken cancellationToken
    )
    {
        string processName = windowInfo.ProcessName;
        string? executablePath = windowInfo.ExecutablePath;

        UserSettings settings = await context
            .UserSettings.AsNoTracking()
            .SingleAsync(cancellationToken: cancellationToken);
        App? app = await context.Apps.FirstOrDefaultAsync(
            p => p.ProcessName == processName,
            cancellationToken
        );

        // 新增 App 信息
        if (app is null)
        {
            if (executablePath is null)
                app = App.CreateDiscovered(occurredAt, processName, processName, executablePath);
            else
            {
                using ExecutableMetadata metadata = executableMetadataProvider.GetMetadata(
                    executablePath
                );
                string name = metadata.Name is null ? processName : metadata.Name;
                app = App.CreateDiscovered(occurredAt, name, processName, executablePath);
                string? iconPath = await EnsureIconUpdated(
                    app,
                    metadata,
                    settings.AppTracking.IconDirectory,
                    cancellationToken
                );
                app.RefreshMetadata(occurredAt, executablePath, iconPath);
            }
            context.Apps.Add(app);
            await context.SaveChangesAsync(cancellationToken);
        }
        // 已有 App 信息
        else
        {
            if (app.NeedsMetadataRefresh(occurredAt, settings.AppTracking.MetadataStaleThreshold))
            {
                if (executablePath is null)
                    app.RefreshMetadata(occurredAt, executablePath, null);
                else
                {
                    using ExecutableMetadata metadata = executableMetadataProvider.GetMetadata(
                        executablePath
                    );
                    string? iconPath = await EnsureIconUpdated(
                        app,
                        metadata,
                        settings.AppTracking.IconDirectory,
                        cancellationToken
                    );
                    app.RefreshMetadata(occurredAt, executablePath, iconPath);
                }
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        return app.Id;
    }

    private static async Task<string?> EnsureIconUpdated(
        App app,
        ExecutableMetadata metadata,
        string iconDir,
        CancellationToken cancellationToken
    )
    {
        if (metadata.IconStream is null)
        {
            CleanUpManagedIcon(app.IconPath, iconDir);
            return null;
        }

        string newIconPath = Path.Combine(
            iconDir,
            $"{app.ProcessName}.{metadata.IconFileExtension}"
        );

        StringComparison pathComparison =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        bool isSamePath = string.Equals(app.IconPath, newIconPath, pathComparison);

        // 如果新旧路径不一致，且旧图标属于受管理目录，则清理旧图标
        if (!isSamePath)
            CleanUpManagedIcon(app.IconPath, iconDir);
        // 简单校验，如果路径一致且新旧图标大小相同，跳过 IO 操作
        else if (
            File.Exists(newIconPath)
            && new FileInfo(newIconPath).Length == metadata.IconStream.Length
        )
            return newIconPath;

        // 确保目录存在
        Directory.CreateDirectory(iconDir);

        // 使用 FileMode.Create 自动覆盖旧文件
        await using (
            var fileStream = new FileStream(
                newIconPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            )
        )
            await metadata.IconStream.CopyToAsync(fileStream, cancellationToken);

        return newIconPath;
    }

    /// 仅清理在受管理目录（managedDirectory）下的旧文件
    private static void CleanUpManagedIcon(string? filePath, string managedDirectory)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        // 只有文件确实在软件管理的图标目录下时，才允许删除
        if (IsFileInDirectory(filePath, managedDirectory))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // 忽略删除失败的异常（如文件被占用）
            }
        }
    }

    // 判断文件路径是否位于指定目录下
    private static bool IsFileInDirectory(string filePath, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(directoryPath))
            return false;

        try
        {
            // 获取相对路径（Path.GetRelativePath 会自动处理 Windows/macOS/Linux 的大小写规则）
            string relativePath = Path.GetRelativePath(directoryPath, filePath);

            // 满足以下条件说明 filePath 确实在 directoryPath 目录（或子目录）下：
            //    - 相对路径不等于 "." (代表就是目录本身)
            //    - 相对路径不以 ".." 开头 (代表不在父级或平级目录)
            //    - 相对路径不是根绝对路径 (代表没有跨驱动器，比如从 C:\ 到 D:\)
            return relativePath != "."
                && !relativePath.StartsWith("..", StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath);
        }
        catch
        {
            return false;
        }
    }
}

public interface IExecutableMetadataProvider
{
    public ExecutableMetadata GetMetadata(string executablePath);
}

public class ExecutableMetadata(string? name, Stream? iconStream, string? iconFileExtension)
    : IDisposable
{
    public string? Name { get; } = name;
    public Stream? IconStream { get; } = iconStream;
    public string? IconFileExtension { get; } = iconFileExtension;

    public void Dispose()
    {
        IconStream?.Dispose();
        GC.SuppressFinalize(this);
    }
}
