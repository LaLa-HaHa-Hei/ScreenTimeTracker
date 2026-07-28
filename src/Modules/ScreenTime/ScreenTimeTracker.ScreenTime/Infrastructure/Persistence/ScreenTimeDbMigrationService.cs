using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Domain;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

public partial class ScreenTimeDbMigrationService(
    ILogger<ScreenTimeDbMigrationService> logger,
    TimeProvider timeProvider,
    IServiceScopeFactory scopeFactory
) : IHostedLifecycleService
{
    // 在所有服务启动之前执行
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        LogScreenTimeDbMigrationServiceStarting(logger);

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
            DateTime now = timeProvider.GetLocalNow().DateTime;
            await SeedDefaultCategoriesAsync(context, now, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "ScreenTimeDbMigrationService is starting."
    )]
    private static partial void LogScreenTimeDbMigrationServiceStarting(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "New database detected, seeding default categories."
    )]
    private static partial void LogNewDatabaseDetected(ILogger logger);

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
        DateTime now,
        CancellationToken cancellationToken
    )
    {
        var defaults = new List<AppCategory>
        {
            AppCategory.Create(now, "Game", "#F2A65A", "./PrePreparedAppCategoryIcons/game.svg"),
            AppCategory.Create(now, "Relax", "#65B891", "./PrePreparedAppCategoryIcons/relax.svg"),
            AppCategory.Create(
                now,
                "Social",
                "#D878A8",
                "./PrePreparedAppCategoryIcons/social.svg"
            ),
            AppCategory.Create(now, "Study", "#4A90E2", "./PrePreparedAppCategoryIcons/study.svg"),
            AppCategory.Create(now, "Video", "#E76F7A", "./PrePreparedAppCategoryIcons/video.svg"),
            AppCategory.Create(now, "Work", "#7C6FF6", "./PrePreparedAppCategoryIcons/work.svg"),
        };

        await context.AppCategories.AddRangeAsync(defaults, cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
