using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.Application.Configuration;
using ScreenTimeTracker.Domain.Entities;
using ScreenTimeTracker.Domain.Interfaces;

namespace ScreenTimeTracker.Application.Services
{
    public class AggregationService(IServiceScopeFactory scopeFactory, ILogger<AggregationService> logger, IOptions<TrackerOptions> trackerOptions)
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<AggregationService> _logger = logger;
        private readonly IOptions<TrackerOptions> _trackerOptions = trackerOptions;

        public async Task SummarizeHourlyDataAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var activityIntervalRepository = scope.ServiceProvider.GetRequiredService<IActivityIntervalRepository>();
            var hourlySummaryRepository = scope.ServiceProvider.GetRequiredService<IHourlySummaryRepository>();

            var now = DateTime.UtcNow;
            DateTime adjusted = now.AddMinutes(-_trackerOptions.Value.IdleTimeoutMinutes);
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
}
