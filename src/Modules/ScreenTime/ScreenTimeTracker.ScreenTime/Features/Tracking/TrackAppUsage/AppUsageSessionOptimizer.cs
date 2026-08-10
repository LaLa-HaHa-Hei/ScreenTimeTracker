using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public partial class AppUsageSessionOptimizer(
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

                var queryStartTime = anchor?.StartTime ?? firstUnoptimized.StartTime;

                // 拉取待处理窗口内的所有数据
                var sessions = await context
                    .AppUsageSessions.Where(s =>
                        queryStartTime <= s.StartTime && s.EndTime <= cutoffTime
                    )
                    .OrderBy(s => s.StartTime)
                    .ToListAsync(stoppingToken);

                if (sessions.Count == 0)
                    return;

                var sessionsToRemove = new HashSet<AppUsageSession>();

                // 执行合并逻辑
                for (int i = 0; i < sessions.Count; i++)
                {
                    var current = sessions[i];
                    for (int j = i + 1; j < sessions.Count; j++)
                    {
                        var next = sessions[j];
                        if (
                            next.StartTime
                            > current.EndTime + settings.AppTracking.UsageSessionMergeTolerance
                        )
                            break;
                        if (current.AppId == next.AppId)
                        {
                            current.UpdateEndTime(next.EndTime); // 扩展结束时间
                            // 删除 (i, j] 之间的所有杂项
                            for (int k = i + 1; k <= j; k++)
                                sessionsToRemove.Add(sessions[k]);
                            i = j;
                            // 不要break，因为此时current的EndTime变长了，可以继续查找合并的记录
                        }
                    }
                }

                // 过滤并标记状态
                foreach (var session in sessions)
                {
                    if (sessionsToRemove.Contains(session))
                        continue; // 已经被合并废弃的不管

                    var duration = session.EndTime - session.StartTime;

                    // 如果时长小于最小有效时长则删除
                    if (duration < settings.AppTracking.MinValidUsageSessionDuration)
                        sessionsToRemove.Add(session);
                    // 成功存活下来的记录标记为已优化
                    else
                        session.MarkAsOptimized();
                }

                // 提交到数据库
                if (sessionsToRemove.Count != 0)
                    context.AppUsageSessions.RemoveRange(sessionsToRemove);

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
