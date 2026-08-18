using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.UserSettings;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public partial class AppUsageSessionOptimizationWorker(
    ActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = await GetUserSettingsAsync(stoppingToken);
        using var timer = new PeriodicTimer(settings.AppTracking.UsageSessionOptimizationInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var latestSettings = await GetUserSettingsAsync(stoppingToken);
                if (
                    latestSettings.AppTracking.UsageSessionOptimizationInterval
                    != settings.AppTracking.UsageSessionOptimizationInterval
                )
                    timer.Period = latestSettings.AppTracking.UsageSessionOptimizationInterval;

                settings = latestSettings;

                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();

                // 安全边界：不干涉ActiveSession因自动保存的数据，也不影响空闲检测
                var now = timeProvider.GetUtcNow();
                var safeBuffer = TimeSpan.FromMinutes(1);
                var activeSessionElapsedTime = activeSessionStore.Current is null
                    ? TimeSpan.Zero
                    : now - activeSessionStore.Current.StartTime;
                var cutoffTime =
                    now
                    - safeBuffer
                    - (
                        activeSessionElapsedTime > settings.IdleDetection.InactivityThreshold
                            ? activeSessionElapsedTime
                            : settings.IdleDetection.InactivityThreshold
                    );

                // 查找第一条未优化的记录
                var firstUnoptimized = await context
                    .AppUsageSessions.Where(s => !s.IsOptimized && s.EndTime <= cutoffTime)
                    .OrderBy(s => s.StartTime)
                    .FirstOrDefaultAsync(stoppingToken);

                if (firstUnoptimized is null)
                    continue; // 没有需要优化的数据，直接返回

                // 为了防止跨批次断层，拉取第一条未优化记录之前的“最后一条已优化的记录”
                var anchor = await context
                    .AppUsageSessions.Where(s =>
                        s.IsOptimized && s.EndTime <= firstUnoptimized.StartTime
                    )
                    .OrderByDescending(s => s.EndTime)
                    .FirstOrDefaultAsync(stoppingToken);

                var queryStartTime =
                    anchor?.UsagePeriod.Start ?? firstUnoptimized.UsagePeriod.Start;

                // 拉取待处理窗口内的所有数据
                var sessions = await context
                    .AppUsageSessions.Where(s =>
                        queryStartTime <= s.StartTime && s.EndTime <= cutoffTime
                    )
                    .OrderBy(s => s.StartTime)
                    .ToListAsync(stoppingToken);

                if (sessions.Count == 0)
                    continue;

                var result = AppUsageSessionOptimizer.Optimize(
                    sessions,
                    settings.AppTracking.UsageSessionMergeTolerance,
                    settings.AppTracking.MinValidUsageSessionDuration
                );

                if (result.SessionsToRemove.Count != 0)
                    context.AppUsageSessions.RemoveRange(result.SessionsToRemove);

                await context.SaveChangesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
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
