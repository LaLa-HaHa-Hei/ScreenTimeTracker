namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public record TimerDriftDetectedEvent(DateTimeOffset DriftStartAt) : INotification;

public partial class TimerDriftMonitor(
    ILogger<TimerDriftMonitor> logger,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    private static readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan _driftThreshold = TimeSpan.FromSeconds(3);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_checkInterval);
        var lastTickTime = DateTimeOffset.UtcNow;

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var now = DateTimeOffset.UtcNow;
            var actualElapsed = now - lastTickTime;

            if (actualElapsed > _checkInterval + _driftThreshold)
            {
                if (logger.IsEnabled(LogLevel.Information))
#pragma warning disable CA1873 // Avoid potentially expensive logging
                    LogTimerDriftDetected(logger, lastTickTime.ToLocalTime());
#pragma warning restore CA1873 // Avoid potentially expensive logging

                using var scope = scopeFactory.CreateScope();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
                await publisher.Publish(new TimerDriftDetectedEvent(lastTickTime), stoppingToken);
            }

            lastTickTime = now;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Timer drift detected. Drift start: {DriftStartAt}."
    )]
    private static partial void LogTimerDriftDetected(ILogger logger, DateTimeOffset driftStartAt);
}
