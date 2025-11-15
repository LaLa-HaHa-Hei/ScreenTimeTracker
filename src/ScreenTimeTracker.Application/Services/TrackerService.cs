using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.Application.Configuration;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Domain.Entities;
using ScreenTimeTracker.Domain.Interfaces;
using System.Diagnostics;

namespace ScreenTimeTracker.Application.Services
{
    public class TrackerService(IServiceScopeFactory scopeFactory, ILogger<TrackerService> logger, IForegroundWindowService foregroundWindowService, IIdleTimeProvider idleTimeProvider, IOptions<TrackerOptions> options)
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly ILogger<TrackerService> _logger = logger;
        private readonly IForegroundWindowService _foregroundWindowService = foregroundWindowService;
        private readonly IIdleTimeProvider _idleTimeProvider = idleTimeProvider;
        private readonly IOptions<TrackerOptions> _options = options;

        public async Task RecordActivityIntervalAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var activityIntervalRepository = scope.ServiceProvider.GetRequiredService<IActivityIntervalRepository>();
            var processManagementService = scope.ServiceProvider.GetRequiredService<ProcessManagementService>();
            var now = DateTime.UtcNow;
            var duration = TimeSpan.FromMilliseconds(_options.Value.PollingIntervalMilliseconds);
            TimeSpan idleTime = await _idleTimeProvider.GetSystemIdleTimeAsync();
            ProcessInfo processInfo;

            // 检查用户空闲，并把空闲开始到现在的记录为空闲时间
            if (idleTime >= TimeSpan.FromMinutes(_options.Value.IdleTimeoutMinutes))
            {
                processInfo = await processManagementService.EnsureIdleProcessInfoExistsAsync();

                DateTime idleStartTime = now.Add(-idleTime);
                IEnumerable<ActivityInterval> activeIntervals = await activityIntervalRepository.GetNonIdleByTimestampAfterAsync(idleStartTime);

                await activityIntervalRepository.UpdateRangeAsync(activeIntervals.Select(i =>
                {
                    i.UpdateProcess(processInfo);
                    return i;
                }));
            }
            // 添加新记录
            else
            {
                Process? process = await _foregroundWindowService.GetForegroundProcessAsync();
                processInfo = await processManagementService.EnsureProcessInfoExistsAsync(process);
            }

            await activityIntervalRepository.AddAsync(new ActivityInterval(Guid.NewGuid(), now, processInfo, duration));
        }
    }
}
