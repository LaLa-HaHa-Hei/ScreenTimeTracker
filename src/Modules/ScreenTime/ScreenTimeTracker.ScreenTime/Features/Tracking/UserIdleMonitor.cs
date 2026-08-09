using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public partial class UserIdleMonitor(
    ILogger<UserIdleMonitor> logger,
    UserIdleStore userIdleStore,
    TimeProvider timeProvider,
    IIdleTimeProvider idleTimeProvider,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var userSettings = await GetUserSettingsAsync(stoppingToken);
        using var timer = new PeriodicTimer(userSettings.IdleDetection.PollingInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var latestSettings = await GetUserSettingsAsync(stoppingToken);
                if (
                    latestSettings.IdleDetection.PollingInterval
                    != userSettings.IdleDetection.PollingInterval
                )
                    timer.Period = latestSettings.IdleDetection.PollingInterval;

                // 后续要用到 settings 中的其他属性，这里一定更新
                userSettings = latestSettings;

                // 空闲检测未启用，跳过
                if (!userSettings.IdleDetection.IsEnabled)
                    continue;

                var now = timeProvider.GetUtcNow();
                var systemIdleTime = await idleTimeProvider.GetSystemIdleTimeAsync();
                // 处于空闲状态
                if (systemIdleTime >= userSettings.IdleDetection.InactivityThreshold)
                {
                    // 从活跃状态到空闲状态
                    if (!userIdleStore.Current.IsUserIdle)
                    {
                        var idleStartedAt = now - systemIdleTime;
                        LogUserBecameIdle(logger, idleStartedAt);
                        userIdleStore.Current = new(true);
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(
                            new TrackAppUsage.UserIdleCommand(idleStartedAt),
                            stoppingToken
                        );
                        await mediator.Send(
                            new TrackWebsiteUsage.UserIdleCommand(idleStartedAt),
                            stoppingToken
                        );
                    }
                }
                // 处于活跃状态
                else
                {
                    // 从空闲状态到活跃状态
                    if (userIdleStore.Current.IsUserIdle)
                    {
                        LogUserBecameActive(logger);
                        userIdleStore.Current = new(false);
                        using var scope = scopeFactory.CreateScope();
                        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                        await mediator.Send(new TrackAppUsage.UserActiveCommand(), stoppingToken);
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "User has become idle. Idle started at {idleStartedAt}."
    )]
    private static partial void LogUserBecameIdle(ILogger logger, DateTimeOffset idleStartedAt);

    [LoggerMessage(Level = LogLevel.Information, Message = "User has become active.")]
    private static partial void LogUserBecameActive(ILogger logger);

    private async Task<UserSettings> GetUserSettingsAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        ScreenTimeDbContext context =
            scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        var userSettings = await context.UserSettings.AsNoTracking().SingleAsync(stoppingToken);
        return userSettings;
    }
}
