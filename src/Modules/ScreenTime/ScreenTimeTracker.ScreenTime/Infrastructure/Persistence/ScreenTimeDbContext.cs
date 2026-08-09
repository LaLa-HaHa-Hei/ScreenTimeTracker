using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ScreenTimeTracker.BuildingBlocks.Persistence;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Domain.Websites;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

public class ScreenTimeDbContext(DbContextOptions<ScreenTimeDbContext> options)
    : ModuleDbContext(options, "ScreenTime")
{
    public DbSet<AppUsageSession> AppUsageSessions { get; set; }
    public DbSet<App> Apps { get; set; }
    public DbSet<AppCategory> AppCategories { get; set; }
    public DbSet<WebsiteUsageSession> WebsiteUsageSessions { get; set; }
    public DbSet<Website> Websites { get; set; }
    public DbSet<WebsiteCategory> WebsiteCategories { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToUnixMsConverter>();
        base.ConfigureConventions(configurationBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ScreenTimeDbContext).Assembly);
    }
}

public class DateTimeOffsetToUnixMsConverter : ValueConverter<DateTimeOffset, long>
{
    public DateTimeOffsetToUnixMsConverter()
        : base(v => v.ToUnixTimeMilliseconds(), v => DateTimeOffset.FromUnixTimeMilliseconds(v)) { }
}
