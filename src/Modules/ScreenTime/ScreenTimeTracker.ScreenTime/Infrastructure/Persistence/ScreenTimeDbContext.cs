using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.BuildingBlocks.Persistence;
using ScreenTimeTracker.ScreenTime.Domain;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

public class ScreenTimeDbContext(DbContextOptions<ScreenTimeDbContext> options)
    : ModuleDbContext(options, "ScreenTime")
{
    public DbSet<AppUsageSession> AppUsageSessions { get; set; }
    public DbSet<App> Apps { get; set; }
    public DbSet<AppCategory> AppCategories { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 自动应用同程序集内所有 IEntityTypeConfiguration<>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScreenTimeDbContext).Assembly);
    }
}
