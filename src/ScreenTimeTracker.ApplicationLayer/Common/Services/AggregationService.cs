using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Common.Services;

public class AggregationService(IServiceScopeFactory scopeFactory, ILogger<AggregationService> logger)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<AggregationService> _logger = logger;


    public async Task SummarizeHourlyDataAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var activityIntervalRepository = scope.ServiceProvider.GetRequiredService<IActivityIntervalRepository>();
        var hourlySummaryRepository = scope.ServiceProvider.GetRequiredService<IHourlySummaryRepository>();
        var userConfigurationRepository = scope.ServiceProvider.GetRequiredService<IUserConfigurationRepository>();

        var now = DateTime.Now;
        TrackerSettings trackerSettings = (await userConfigurationRepository.GetConfig()).Tracker;
        DateTime adjusted = now.Add(-trackerSettings.IdleTimeout);
        DateTime dataFreezeTime = new(adjusted.Year, adjusted.Month, adjusted.Day, adjusted.Hour, 0, 0);

        IEnumerable<ActivityInterval> activeIntervalsBefore = await activityIntervalRepository.GetByTimestampBeforeAsync(dataFreezeTime);

        IEnumerable<HourlySummary> hourlySummaries = activeIntervalsBefore
            .GroupBy(i => new
            {
                i.TrackedProcess,
                Hour = new DateTime(i.Timestamp.Year, i.Timestamp.Month, i.Timestamp.Day, i.Timestamp.Hour, 0, 0)
            })
            .Select(g => new HourlySummary(g.Key.TrackedProcess, g.Key.Hour, TimeSpan.FromTicks(g.Sum(x => x.Duration.Ticks))));

        foreach (var summaryCandidate in hourlySummaries)
        {
            var existingSummary = await hourlySummaryRepository.GetByProcessAndHourAsync(summaryCandidate.TrackedProcess, summaryCandidate.Hour);

            if (existingSummary == null)
            {
                await hourlySummaryRepository.AddAsync(summaryCandidate);
            }
            else
            {
                existingSummary.AddDuration(summaryCandidate.TotalDuration);
                await hourlySummaryRepository.UpdateAsync(existingSummary);
            }
        }
        await activityIntervalRepository.RemoveRangeAsync(activeIntervalsBefore);
    }
}
