using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public record ForegroundWindowChangedCommand(WindowInfo? WindowInfo) : IRequest;

public class ForegroundWindowChangedHandler(
    ScreenTimeDbContext context,
    ActiveAppUsageSessionStore activeSessionStore,
    IExecutableMetadataProvider executableMetadataProvider,
    TimeProvider timeProvider
) : IRequestHandler<ForegroundWindowChangedCommand>
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public async ValueTask<Unit> Handle(
        ForegroundWindowChangedCommand request,
        CancellationToken cancellationToken
    )
    {
        await _lock.WaitAsync(cancellationToken);

        try
        {
            var now = timeProvider.GetUtcNow();

            Guid appId;
            if (request.WindowInfo is null)
                appId = App.UnknownAppId;
            else
                appId = await IdentifyApp(now, request.WindowInfo, cancellationToken);

            if (activeSessionStore.Current is null)
            {
                activeSessionStore.Current = new ActiveAppUsageSessionState(appId, now);
                return Unit.Value;
            }
            else if (activeSessionStore.Current.AppId == appId)
                return Unit.Value;
            else
            {
                await activeSessionStore.Current.PersistSessionAsync(
                    context,
                    now,
                    cancellationToken
                );
                activeSessionStore.Current = new ActiveAppUsageSessionState(appId, now);
                await context.SaveChangesAsync(cancellationToken);
            }
            return Unit.Value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Guid> IdentifyApp(
        DateTimeOffset now,
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
                app = App.CreateDiscovered(now, processName, processName, executablePath);
            else
            {
                using ExecutableMetadata metadata =
                    await executableMetadataProvider.GetMetadataAsync(executablePath);
                string name = string.IsNullOrWhiteSpace(metadata.Name)
                    ? processName
                    : metadata.Name;
                app = App.CreateDiscovered(now, name, processName, executablePath);
                string? iconPath = await EnsureIconUpdated(
                    app,
                    metadata,
                    settings,
                    cancellationToken
                );
                app.RefreshMetadata(now, executablePath, iconPath);
            }
            context.Apps.Add(app);
            await context.SaveChangesAsync(cancellationToken);
        }
        // 已有 App 信息
        else
        {
            if (app.NeedsMetadataRefresh(now, settings.AppTracking.MetadataStaleThreshold))
            {
                if (executablePath is null)
                    app.RefreshMetadata(now, executablePath, null);
                else
                {
                    using ExecutableMetadata metadata =
                        await executableMetadataProvider.GetMetadataAsync(executablePath);
                    string? iconPath = await EnsureIconUpdated(
                        app,
                        metadata,
                        settings,
                        cancellationToken
                    );
                    app.RefreshMetadata(now, executablePath, iconPath);
                }
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        return app.Id;
    }

    private static async Task<string?> EnsureIconUpdated(
        App app,
        ExecutableMetadata metadata,
        UserSettings settings,
        CancellationToken cancellationToken
    )
    {
        if (metadata.IconStream is null)
        {
            if (File.Exists(app.IconPath))
                File.Delete(app.IconPath);
            return null;
        }

        // 如果新旧图标大小相同，跳过 IO 操作（简单校验）
        if (
            File.Exists(app.IconPath)
            && new FileInfo(app.IconPath).Length == metadata.IconStream.Length
        )
        {
            return app.IconPath;
        }

        // 确保目录存在
        Directory.CreateDirectory(settings.AppTracking.IconDirectory);

        string newIconPath = Path.Combine(
            settings.AppTracking.IconDirectory,
            $"{app.ProcessName}.{metadata.IconFileExtension}"
        );

        // 使用 FileMode.Create 自动覆盖旧文件
        await using (
            var fileStream = new FileStream(
                newIconPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            )
        )
        {
            await metadata.IconStream.CopyToAsync(fileStream, cancellationToken);
        }

        return newIconPath;
    }
}

public interface IExecutableMetadataProvider
{
    public Task<ExecutableMetadata> GetMetadataAsync(string executablePath);
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
