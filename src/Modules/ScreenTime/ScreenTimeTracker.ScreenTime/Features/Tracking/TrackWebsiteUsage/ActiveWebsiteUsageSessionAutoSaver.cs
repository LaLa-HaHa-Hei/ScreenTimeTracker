using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;

public partial class ActiveWebsiteUsageSessionAutoSaver(
    IServiceScopeFactory scopeFactory,
    ActiveWebsiteUsageSessionStore activeSessionStore
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = await GetAutoSaveIntervalAsync(stoppingToken);
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var latestInterval = await GetAutoSaveIntervalAsync(stoppingToken);
                if (latestInterval != interval)
                {
                    interval = latestInterval;
                    timer.Period = interval;
                }

                if (activeSessionStore.Current is null)
                    continue;

                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
                await context.PersistActiveSessionAsync(
                    activeSessionStore.Current,
                    activeSessionStore.Current.LastActiveAt,
                    cancellationToken: stoppingToken
                );
                await context.SaveChangesAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task<TimeSpan> GetAutoSaveIntervalAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        ScreenTimeDbContext context =
            scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        var userSettings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        return userSettings.WebsiteTracking.ActiveUsageSessionAutoSaveInterval;
    }
}
