using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.DbContexts
{
    public class ScreenTimeDbContext(DbContextOptions<ScreenTimeDbContext> options) : DbContext(options)
    {
        public DbSet<ProcessInfoEntity> ProcessInfos => Set<ProcessInfoEntity>();
        public DbSet<ActivityIntervalEntity> ActivityIntervals => Set<ActivityIntervalEntity>();
        public DbSet<HourlySummaryEntity> HourlySummaries => Set<HourlySummaryEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // 自动应用同程序集内所有 IEntityTypeConfiguration<>
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScreenTimeDbContext).Assembly);
        }
    }
}
