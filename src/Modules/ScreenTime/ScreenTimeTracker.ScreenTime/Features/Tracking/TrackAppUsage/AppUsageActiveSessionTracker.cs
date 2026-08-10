using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public partial class AppUsageActiveSessionTracker(
    IServiceScopeFactory scopeFactory,
    ActiveAppUsageSessionStore activeSessionStore,
    IForegroundWindowMonitor foregroundWindowMonitor,
    UserIdleStore userIdleStore,
    SystemSuspendStore systemSuspendStore,
    TimeProvider timeProvider
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = scopeFactory.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
            await mediator.Send(new ForegroundWindowChangedCommand(windowInfo), stoppingToken);
        }

        foregroundWindowMonitor.ForegroundWindowChanged += OnForegroundWindowChanged;

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }

        // 空闲检测循环
        foregroundWindowMonitor.ForegroundWindowChanged -= OnForegroundWindowChanged;

        // 退出时保存当前会话数据
        if (activeSessionStore.Current is null)
            return;
        using (var scope = scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
            var now = timeProvider.GetUtcNow();
            await activeSessionStore.Current.PersistSessionAsync(
                context,
                now,
                CancellationToken.None
            );
            activeSessionStore.Current = null;
            await context.SaveChangesAsync(CancellationToken.None);
        }
    }

    private async void OnForegroundWindowChanged(object? sender, WindowInfo? windowInfo)
    {
        if (userIdleStore.Current.IsUserIdle || systemSuspendStore.Current.IsSystemSuspendActive)
            return;

        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        await mediator.Send(new ForegroundWindowChangedCommand(windowInfo));
    }
}
