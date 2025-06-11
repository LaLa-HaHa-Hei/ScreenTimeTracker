using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Data
{
    public class ScreenTimeContextFactory : IDesignTimeDbContextFactory<ScreenTimeContext>
    {
        public ScreenTimeContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ScreenTimeContext>();
            optionsBuilder.UseSqlite("Data Source=Date/Data.db");
            return new ScreenTimeContext(optionsBuilder.Options);
        }
    }
}
