using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsageSession;

public partial class AppUsageActiveSessionTracker(
    ILogger<AppUsageActiveSessionTracker> logger,
    IServiceScopeFactory scopeFactory,
    IActiveAppUsageSessionStore activeSessionStore,
    IForegroundWindowMonitor foregroundWindowMonitor,
    IIdleTimeProvider idleTimeProvider,
    TimeProvider timeProvider
) : BackgroundService
{
    private DateTime? _idleStartedAt;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogActiveSessionTrackerStarting(logger);

        using (var scope = scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
            await mediator.Send(new ForegroundWindowChangedCommand(windowInfo), stoppingToken);
        }

        foregroundWindowMonitor.ForegroundWindowChanged += OnForegroundWindowChanged;

        var idleDetectionTask = RunIdleDetectionLoopAsync(stoppingToken);
        var timeJumpDetectionTask = RunSystemSuspendResumeDetectionLoopAsync(stoppingToken);

        await Task.WhenAll(idleDetectionTask, timeJumpDetectionTask);

        // 空闲检测循环
        foregroundWindowMonitor.ForegroundWindowChanged -= OnForegroundWindowChanged;

        // 退出时保存当前会话数据
        if (activeSessionStore.Current is null)
            return;
        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
            var now = timeProvider.GetLocalNow().DateTime;
            await activeSessionStore.StageSaveToAsync(context, now, stoppingToken);
            activeSessionStore.Current = null;
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "ActiveSessionTracker is starting.")]
    private static partial void LogActiveSessionTrackerStarting(ILogger logger);

    // 空闲检测循环
    private async Task RunIdleDetectionLoopAsync(CancellationToken cancellationToken)
    {
        var settings = await GetUserSettingsAsync(cancellationToken);
        using var timer = new PeriodicTimer(settings.IdleDetectionPollingInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var latestSettings = await GetUserSettingsAsync(cancellationToken);
                if (
                    latestSettings.IdleDetectionPollingInterval
                    != settings.IdleDetectionPollingInterval
                )
                    timer.Period = latestSettings.IdleDetectionPollingInterval;

                // 后续要用到 settings 中的其他属性，这里一定更新
                settings = latestSettings;

                // 空闲检测未启用，跳过
                if (!settings.IsIdleDetectionEnabled)
                    continue;

                var now = timeProvider.GetLocalNow().DateTime;
                var systemIdleTime = await idleTimeProvider.GetSystemIdleTimeAsync();
                // 处于空闲状态
                if (systemIdleTime >= settings.IdleThreshold)
                {
                    // 从活跃状态到空闲状态
                    if (_idleStartedAt is null)
                    {
                        _idleStartedAt = now - systemIdleTime;
                        LogSystemBecameIdle(logger, _idleStartedAt.Value);
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(
                            new SystemBecameIdleCommand(_idleStartedAt.Value),
                            cancellationToken
                        );
                    }
                }
                // 处于活跃状态
                else
                {
                    // 从空闲状态到活跃状态
                    if (_idleStartedAt is not null)
                    {
                        _idleStartedAt = null;
                        LogSystemBecameActive(logger);
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
                        await mediator.Send(
                            new ForegroundWindowChangedCommand(windowInfo),
                            cancellationToken
                        );
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System has become idle. Idle started at {IdleStartedAt}."
    )]
    private static partial void LogSystemBecameIdle(ILogger logger, DateTime idleStartedAt);

    [LoggerMessage(Level = LogLevel.Information, Message = "System has become active.")]
    private static partial void LogSystemBecameActive(ILogger logger);

    // 系统睡眠/恢复检测循环
    private async Task RunSystemSuspendResumeDetectionLoopAsync(CancellationToken cancellationToken)
    {
        var pollingInterval = TimeSpan.FromSeconds(2);
        var jumpThreshold = TimeSpan.FromSeconds(5);

        using var timer = new PeriodicTimer(pollingInterval);
        var lastTickTime = timeProvider.GetLocalNow().DateTime;
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                var currentTickTime = timeProvider.GetLocalNow().DateTime;
                var timeElapsed = currentTickTime - lastTickTime;

                if (timeElapsed > jumpThreshold)
                {
                    LogSystemSuspendedAndResumed(logger, lastTickTime);
                    using var scope = scopeFactory.CreateScope();
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.Send(
                        new SystemResumeFromSuspendCommand(lastTickTime),
                        cancellationToken
                    );

                    var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
                    await mediator.Send(
                        new ForegroundWindowChangedCommand(windowInfo),
                        cancellationToken
                    );
                }

                lastTickTime = currentTickTime;
            }
        }
        catch (OperationCanceledException) { }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System likely suspended at {SuspendTime} and resumed."
    )]
    private static partial void LogSystemSuspendedAndResumed(ILogger logger, DateTime suspendTime);

    private async void OnForegroundWindowChanged(object? sender, WindowInfo? windowInfo)
    {
        // 处于空闲状态时，忽略窗口变化
        if (_idleStartedAt is not null)
            return;

        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new ForegroundWindowChangedCommand(windowInfo));
    }

    private async Task<UserSettings> GetUserSettingsAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        ScreenTimeDbContext context =
            scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        var userSettings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        return userSettings;
    }
}

public interface IForegroundWindowMonitor
{
    WindowInfo? GetForegroundWindow();
    event EventHandler<WindowInfo?> ForegroundWindowChanged;
}

public interface IIdleTimeProvider
{
    Task<TimeSpan> GetSystemIdleTimeAsync();
}
