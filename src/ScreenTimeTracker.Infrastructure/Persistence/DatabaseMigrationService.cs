using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence;

public class DbInitializationService(ILogger<DbInitializationService> logger, IServiceProvider serviceProvider) : IHostedLifecycleService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DbInitializationService> _logger = logger;

    // 在所有服务启动之前执行
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("DbInitializationService is Starting");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ScreenTimeDbContext>();
        // 确保数据库目录存在
        var connectionString = dbContext.Database.GetConnectionString();
        var dbPath = GetDatabasePath(connectionString);

        if (!string.IsNullOrEmpty(dbPath))
        {
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await dbContext.Database.MigrateAsync(cancellationToken: cancellationToken);
        }
    }

    private static string? GetDatabasePath(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return null;

        var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
        return builder.DataSource;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}