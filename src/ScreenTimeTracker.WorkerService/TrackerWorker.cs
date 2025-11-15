using Microsoft.Extensions.Options;
using ScreenTimeTracker.Application.Configuration;
using ScreenTimeTracker.Application.Services;

namespace ScreenTimeTracker.WorkerService;

public class TrackerWorker(ILogger<TrackerWorker> logger, TrackerService trackerService, IOptions<TrackerOptions> options) : BackgroundService
{
    private readonly ILogger<TrackerWorker> _logger = logger;
    private readonly TrackerService _trackerService = trackerService;
    private readonly IOptions<TrackerOptions> _options = options;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(options.Value.PollingIntervalMilliseconds));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrackerWorker is starting.");

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            await _trackerService.RecordActivityIntervalAsync();
        }

        _logger.LogInformation("TrackerWorker is stopping.");
    }
}
