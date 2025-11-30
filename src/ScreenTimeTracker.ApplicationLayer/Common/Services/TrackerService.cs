using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ApplicationLayer.Common.Interfaces;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;
using System.Diagnostics;

namespace ScreenTimeTracker.ApplicationLayer.Common.Services;

public class TrackerService(IServiceScopeFactory scopeFactory, ILogger<TrackerService> logger, IForegroundWindowService foregroundWindowService, IIdleTimeProvider idleTimeProvider)
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<TrackerService> _logger = logger;
    private readonly IForegroundWindowService _foregroundWindowService = foregroundWindowService;
    private readonly IIdleTimeProvider _idleTimeProvider = idleTimeProvider;
    private bool _wasPreviouslyIdle = false;

    public async Task RecordActivityIntervalAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var activityIntervalRepository = scope.ServiceProvider.GetRequiredService<IActivityIntervalRepository>();
        var processManagementService = scope.ServiceProvider.GetRequiredService<ProcessInfoManagementService>();
        var userConfigurationRepository = scope.ServiceProvider.GetRequiredService<IUserConfigurationRepository>();
        var now = DateTime.Now;
        TrackerSettings trackerSettings = (await userConfigurationRepository.GetConfig()).Tracker;
        var duration = trackerSettings.PollingInterval;
        TimeSpan idleTime = await _idleTimeProvider.GetSystemIdleTimeAsync();
        ProcessInfo processInfo;

        // 启用空闲检查并且空闲时间超过阈值，标记为空闲进程
        if (trackerSettings.EnableIdleDetection && idleTime >= trackerSettings.IdleTimeout)
        {
            processInfo = await processManagementService.EnsureIdleProcessInfoExistsAsync();

            // 如果之前不是空闲，把真实空闲到现在的全标记为空闲
            if (!_wasPreviouslyIdle)
            {
                _wasPreviouslyIdle = true;
                DateTime idleStartTime = now.Add(-idleTime);
                _logger.LogInformation("System has become idle. IdleStartTime: {IdleStartTime}.", idleStartTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                IEnumerable<ActivityInterval> activeIntervals = await activityIntervalRepository.GetNonIdleByTimestampAfterAsync(idleStartTime);

                await activityIntervalRepository.UpdateRangeAsync(activeIntervals.Select(i =>
                {
                    i.UpdateProcess(processInfo);
                    return i;
                }));
            }

        }
        // 不是空闲，获取顶层窗口对应进程
        else
        {
            if (_wasPreviouslyIdle)
            {
                _wasPreviouslyIdle = false;
                _logger.LogInformation("System has become active.");
            }
            Process? process = await _foregroundWindowService.GetForegroundProcessAsync();
            processInfo = await processManagementService.EnsureProcessInfoExistsAsync(process);
        }

        await activityIntervalRepository.AddAsync(new ActivityInterval(Guid.NewGuid(), now, processInfo, duration));
    }
}

