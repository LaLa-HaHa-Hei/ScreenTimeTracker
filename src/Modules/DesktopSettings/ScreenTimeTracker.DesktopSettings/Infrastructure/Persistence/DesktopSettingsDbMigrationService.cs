using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.DesktopSettings.Domain;

namespace ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

public partial class DesktopSettingsDbMigrationService(
    ILogger<DesktopSettingsDbMigrationService> logger,
    IServiceScopeFactory scopeFactory
) : IHostedLifecycleService
{
    // 在所有服务启动之前执行
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        LogLocalSettingsDbMigrationServiceStarting(logger);

        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DesktopSettingsDbContext>();

        await EnsureDatabaseDirectoryAsync(context);

        var allMigrations = context.Database.GetMigrations();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        bool isNewDatabase = pendingMigrations.Count() == allMigrations.Count();

        await context.Database.MigrateAsync(cancellationToken);

        if (isNewDatabase)
        {
            LogNewDatabaseDetected(logger);
            string osLanguage = CultureInfo.CurrentUICulture.Name;
            if (!LocalSettings.SupportedLanguages.Contains(osLanguage))
                osLanguage = "en-US";
            var appSettings = await context.LocalSettings.SingleAsync(cancellationToken);
            appSettings.Update(language: osLanguage);
            await context.SaveChangesAsync(cancellationToken);
            LogLanguageCorrected(logger, osLanguage);
        }
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Desktop LocalSettingsDbMigrationService is Starting"
    )]
    private static partial void LogLocalSettingsDbMigrationServiceStarting(ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "New database detected, correcting the current language."
    )]
    private static partial void LogNewDatabaseDetected(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Language corrected to: {language}")]
    private static partial void LogLanguageCorrected(ILogger logger, string language);

    private static async Task EnsureDatabaseDirectoryAsync(DesktopSettingsDbContext context)
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

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
