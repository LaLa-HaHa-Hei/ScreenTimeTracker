using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.Infrastructure.Interfaces;

namespace ScreenTimeTracker.Infrastructure.Persistence.DbContexts
{
    public class SqlitePersistenceInitializer(ScreenTimeDbContext dbContext) : IDbContextInitializer
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public async Task InitializeAsync()
        {
            // 确保数据库目录存在
            var connectionString = _dbContext.Database.GetConnectionString();
            var dbPath = GetDatabasePath(connectionString);

            if (!string.IsNullOrEmpty(dbPath))
            {
                var directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await _dbContext.Database.MigrateAsync();
            }
        }

        private static string? GetDatabasePath(string? connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return null;

            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(connectionString);
            return builder.DataSource;
        }
    }
}