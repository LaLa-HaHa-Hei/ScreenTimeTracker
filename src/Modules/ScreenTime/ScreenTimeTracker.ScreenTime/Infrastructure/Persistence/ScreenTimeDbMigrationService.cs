using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Domain.Websites;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

public partial class ScreenTimeDbMigrationService(
    ILogger<ScreenTimeDbMigrationService> logger,
    TimeProvider timeProvider,
    IServiceScopeFactory scopeFactory
) : IHostedLifecycleService
{
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();

        await EnsureDatabaseDirectoryAsync(context);

        var allMigrations = context.Database.GetMigrations();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        bool isNewDatabase = pendingMigrations.Count() == allMigrations.Count();

        await context.Database.MigrateAsync(cancellationToken: cancellationToken);

        if (isNewDatabase)
        {
            LogNewDatabaseDetected(logger);

            DateTimeOffset now = timeProvider.GetUtcNow();
            await SeedDefaultCategoriesAsync(context, now, cancellationToken);

            // 不支持复杂属性的种子数据，所以在这注入
            var userSettings = UserSettings.CreateDefault();
            string? localZoneId = TimeZoneInfo.Local.Id;
            if (OperatingSystem.IsWindows())
                TimeZoneInfo.TryConvertWindowsIdToIanaId(localZoneId, out localZoneId);
            localZoneId ??= "America/New_York";
            userSettings.Update(regional: new RegionalSettings(localZoneId));
            context.UserSettings.Add(userSettings);
            LogTimeZoneIdCorrected(logger, localZoneId);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "New ScreenTime database detected.")]
    private static partial void LogNewDatabaseDetected(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Time zone ID corrected to: {timeZoneId}"
    )]
    private static partial void LogTimeZoneIdCorrected(ILogger logger, string timeZoneId);

    private static async Task EnsureDatabaseDirectoryAsync(ScreenTimeDbContext context)
    {
        var connectionString = context.Database.GetConnectionString();
        if (string.IsNullOrEmpty(connectionString))
            return;
        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        var dbPath = builder.DataSource;

        if (string.IsNullOrEmpty(dbPath))
            return;

        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }

    private static async Task SeedDefaultCategoriesAsync(
        ScreenTimeDbContext context,
        DateTimeOffset now,
        CancellationToken cancellationToken
    )
    {
        var defaultAppCategories = new List<AppCategory>
        {
            AppCategory.Create(now, "Game", "#F2A65A", "./PrePreparedCategoryIcons/game.svg"),
            AppCategory.Create(now, "Relax", "#65B891", "./PrePreparedCategoryIcons/relax.svg"),
            AppCategory.Create(now, "Social", "#D878A8", "./PrePreparedCategoryIcons/social.svg"),
            AppCategory.Create(now, "Study", "#4A90E2", "./PrePreparedCategoryIcons/study.svg"),
            AppCategory.Create(now, "Video", "#E76F7A", "./PrePreparedCategoryIcons/video.svg"),
            AppCategory.Create(now, "Work", "#7C6FF6", "./PrePreparedCategoryIcons/work.svg"),
        };

        var defaultWebsiteCategories = new List<WebsiteCategory>
        {
            WebsiteCategory.Create(now, "Game", "#F2A65A", "./PrePreparedCategoryIcons/game.svg"),
            WebsiteCategory.Create(now, "Relax", "#65B891", "./PrePreparedCategoryIcons/relax.svg"),
            WebsiteCategory.Create(
                now,
                "Social",
                "#D878A8",
                "./PrePreparedCategoryIcons/social.svg"
            ),
            WebsiteCategory.Create(now, "Study", "#4A90E2", "./PrePreparedCategoryIcons/study.svg"),
            WebsiteCategory.Create(now, "Video", "#E76F7A", "./PrePreparedCategoryIcons/video.svg"),
            WebsiteCategory.Create(now, "Work", "#7C6FF6", "./PrePreparedCategoryIcons/work.svg"),
        };

        await context.AppCategories.AddRangeAsync(defaultAppCategories, cancellationToken);
        await context.WebsiteCategories.AddRangeAsync(defaultWebsiteCategories, cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
