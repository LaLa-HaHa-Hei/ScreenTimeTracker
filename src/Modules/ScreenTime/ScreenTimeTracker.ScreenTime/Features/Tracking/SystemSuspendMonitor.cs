using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public record SystemSuspendingEvent : INotification;

public record SystemResumedEvent : INotification;

public partial class SystemSuspendMonitor(
    ILogger<SystemSuspendMonitor> logger,
    ISystemLifecycleProvider systemLifecycleProvider,
    IServiceScopeFactory scopeFactory,
    SystemSuspendStore systemSuspendStore
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        systemLifecycleProvider.Suspending += async (_, _) =>
        {
            LogSystemSuspending(logger);

            systemSuspendStore.Current = new(true);
            using var scope = scopeFactory.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            await publisher.Publish(new SystemSuspendingEvent(), stoppingToken);
        };
        systemLifecycleProvider.Resumed += async (_, _) =>
        {
            LogSystemResumed(logger);

            systemSuspendStore.Current = new(false);
            using var scope = scopeFactory.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            await publisher.Publish(new SystemResumedEvent(), stoppingToken);
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

public interface ISystemLifecycleProvider
{
    event EventHandler? Suspending;
    event EventHandler? Resumed;
    event EventHandler? ShuttingDown;
}
