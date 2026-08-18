using Microsoft.Extensions.Hosting;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public partial class AppUsageActiveSessionTracker(
    IForegroundWindowMonitor foregroundWindowMonitor,
    TimeProvider timeProvider,
    ForegroundWindowProcessor foregroundWindowProcessor
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PushCurrentWindow();

        foregroundWindowMonitor.ForegroundWindowChanged += OnForegroundWindowChanged;

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) { }

        foregroundWindowMonitor.ForegroundWindowChanged -= OnForegroundWindowChanged;
    }

    private void OnForegroundWindowChanged(object? sender, WindowInfo? windowInfo)
    {
        foregroundWindowProcessor.Enqueue(
            new ForegroundWindowChangedMessage(windowInfo, timeProvider.GetUtcNow())
        );
    }

    private void PushCurrentWindow()
    {
        var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
        foregroundWindowProcessor.Enqueue(
            new ForegroundWindowChangedMessage(windowInfo, timeProvider.GetUtcNow())
        );
    }
}
