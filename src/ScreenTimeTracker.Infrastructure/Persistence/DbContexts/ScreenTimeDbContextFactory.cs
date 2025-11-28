using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ScreenTimeTracker.Infrastructure.Persistence.Options;


namespace ScreenTimeTracker.Infrastructure.Persistence.DbContexts
{
    public class ScreenTimeDbContextFactory : IDesignTimeDbContextFactory<ScreenTimeDbContext>
    {
        public ScreenTimeDbContext CreateDbContext(string[] args)
        {
            var options = new DatabaseOptions();
            var optionsBuilder = new DbContextOptionsBuilder<ScreenTimeDbContext>();

            optionsBuilder.UseSqlite($"Data Source={options.DBFilePath}");

            return new ScreenTimeDbContext(optionsBuilder.Options);
        }
    }
}
