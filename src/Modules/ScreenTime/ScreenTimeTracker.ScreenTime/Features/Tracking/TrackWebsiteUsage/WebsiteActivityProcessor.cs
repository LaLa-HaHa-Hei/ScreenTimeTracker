using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage;

public record WebsiteActivityReport(
    string Host,
    string Name,
    TimeSpan Duration,
    DateTimeOffset ReportedAt
);

public partial class WebsiteActivityProcessor(
    ILogger<WebsiteActivityProcessor> logger,
    IServiceScopeFactory serviceScopeFactory,
    UserIdleStore userIdleStore,
    SystemSuspendStore systemSuspendStore
) : BackgroundService
{
    private static readonly TimeSpan AllowedNetworkJitter = TimeSpan.FromSeconds(5);

    private readonly Channel<WebsiteActivityReport> _channel =
        Channel.CreateUnbounded<WebsiteActivityReport>(
            new UnboundedChannelOptions { SingleReader = true }
        );

    public void Enqueue(WebsiteActivityReport report)
    {
        _channel.Writer.TryWrite(report);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var report in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                // 用户空闲或系统挂起时，直接跳过处理
                if (
                    userIdleStore.Current.IsUserIdle
                    || systemSuspendStore.Current.IsSystemSuspendActive
                )
                    continue;

                try
                {
                    await ProcessActivityReportAsync(report, stoppingToken);
                }
                catch (Exception ex)
                {
                    LogProcessActivityFailed(logger, report.Host, ex);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task ProcessActivityReportAsync(
        WebsiteActivityReport report,
        CancellationToken cancellationToken
    )
    {
        using var scope = serviceScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        var activeSessionStore =
            scope.ServiceProvider.GetRequiredService<ActiveWebsiteUsageSessionStore>();

        var website = await context.Websites.FirstOrDefaultAsync(
            w => w.Host == report.Host,
            cancellationToken
        );
        if (website is null)
        {
            website = Website.CreateDiscovered(
                report.ReportedAt - report.Duration,
                report.Name,
                report.Host
            );
            context.Websites.Add(website);
        }

        // 活跃 Session 衔接逻辑
        if (activeSessionStore.Current is null)
            activeSessionStore.Current = new ActiveWebsiteUsageSessionState(
                website.Id,
                report.ReportedAt - report.Duration,
                report.ReportedAt
            );
        else if (
            activeSessionStore.Current.WebsiteId == website.Id
            && (
                activeSessionStore.Current.LastActiveAt + report.Duration - report.ReportedAt
            ).Duration() <= AllowedNetworkJitter
        )
            activeSessionStore.Current = activeSessionStore.Current with
            {
                LastActiveAt = report.ReportedAt,
            };
        else
        {
            // 切换了网站或时间间隔过长：持久化旧 Session 并开启新 Session
            await context.PersistActiveSessionAsync(activeSessionStore.Current, cancellationToken);
            activeSessionStore.Current = new ActiveWebsiteUsageSessionState(
                website.Id,
                report.ReportedAt - report.Duration,
                report.ReportedAt
            );
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to process website activity report for host: {Host}"
    )]
    private static partial void LogProcessActivityFailed(ILogger logger, string host, Exception ex);
}
