using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.BuildingBlocks.Persistence;
using ScreenTimeTracker.DesktopSettings.Domain;

namespace ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

public class DesktopSettingsDbContext(DbContextOptions<DesktopSettingsDbContext> options)
    : ModuleDbContext(options, "DesktopSettings")
{
    public DbSet<LocalSettings> LocalSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 自动应用同程序集内所有 IEntityTypeConfiguration<>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DesktopSettingsDbContext).Assembly);
    }
}
