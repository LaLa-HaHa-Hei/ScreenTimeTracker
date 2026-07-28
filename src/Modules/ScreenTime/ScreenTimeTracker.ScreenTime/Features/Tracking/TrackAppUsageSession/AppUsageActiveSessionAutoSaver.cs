using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsageSession;

public partial class AppUsageActiveSessionAutoSaver(
    ILogger<AppUsageActiveSessionAutoSaver> logger,
    IServiceScopeFactory scopeFactory,
    IActiveAppUsageSessionStore activeSessionStore,
    TimeProvider timeProvider
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        LogActiveSessionAutoSaverStarting(logger);

        var interval = await GetActiveSessionAutoSaveIntervalAsync(stoppingToken);
        using var timer = new PeriodicTimer(interval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                var latestInterval = await GetActiveSessionAutoSaveIntervalAsync(stoppingToken);
                if (latestInterval != interval)
                {
                    interval = latestInterval;
                    timer.Period = interval;
                }

                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
                var now = timeProvider.GetLocalNow().DateTime;
                await activeSessionStore.StageSaveToAsync(context, now, stoppingToken);
            }
        }
        catch (OperationCanceledException) { }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "ActiveSessionAutoSaver is starting.")]
    private static partial void LogActiveSessionAutoSaverStarting(ILogger logger);

    private async Task<TimeSpan> GetActiveSessionAutoSaveIntervalAsync(
        CancellationToken cancellationToken
    )
    {
        using var scope = scopeFactory.CreateScope();
        ScreenTimeDbContext context =
            scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        var userSettings = await context.UserSettings.AsNoTracking().SingleAsync(cancellationToken);
        return userSettings.ActiveAppUsageSessionAutoSaveInterval;
    }
}
