using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public partial class SystemSuspendMonitor(
    ILogger<SystemSuspendMonitor> logger,
    ISystemLifecycleProvider systemLifecycleProvider,
    IServiceScopeFactory scopeFactory
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        systemLifecycleProvider.Suspending += async (_, _) =>
        {
            LogSystemSuspending(logger);

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new SystemSuspendingCommand(), stoppingToken);
        };
        systemLifecycleProvider.Resumed += async (_, _) =>
        {
            LogSystemResumed(logger);

            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Send(new SystemResumedCommand(), stoppingToken);
        };

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "System is suspending.")]
    private static partial void LogSystemSuspending(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "System has resumed.")]
    private static partial void LogSystemResumed(ILogger logger);
}
