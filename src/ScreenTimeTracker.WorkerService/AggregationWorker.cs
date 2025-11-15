using Microsoft.Extensions.Options;
using ScreenTimeTracker.Application.Configuration;
using ScreenTimeTracker.Application.Services;

namespace ScreenTimeTracker.WorkerService;

public class AggregationWorker(ILogger<AggregationWorker> logger, AggregationService aggregationService, IOptions<AggregationOptions> options) : BackgroundService
{
    private readonly ILogger<AggregationWorker> _logger = logger;
    private readonly AggregationService _aggregationService = aggregationService;
    private readonly IOptions<AggregationOptions> _options = options;
    private readonly PeriodicTimer _timer = new(TimeSpan.FromMinutes(options.Value.PollingIntervalMinutes));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AggregationWorker is starting.");

        await _aggregationService.SummarizeHourlyDataAsync();

        while (await _timer.WaitForNextTickAsync(stoppingToken))
        {
            // _logger.LogInformation("Start summarizing hourly data.");
            await _aggregationService.SummarizeHourlyDataAsync();
            // _logger.LogInformation("Finish summarizing hourly data.");
        }

        _logger.LogInformation("AggregationWorker is stopping.");
    }
}