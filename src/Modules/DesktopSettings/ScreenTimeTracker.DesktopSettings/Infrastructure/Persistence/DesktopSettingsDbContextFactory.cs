using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ScreenTimeTracker.BuildingBlocks.Persistence;

namespace ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

public class DesktopSettingsDbContextFactory : IDesignTimeDbContextFactory<DesktopSettingsDbContext>
{
    public DesktopSettingsDbContext CreateDbContext(string[] args)
    {
        var options = new DatabaseOptions();
        var optionsBuilder = new DbContextOptionsBuilder<DesktopSettingsDbContext>();

        optionsBuilder.UseSqlite(
            $"Data Source={options.DBFilePath}",
            x => x.MigrationsHistoryTable("__EFMigrationsHistory_DesktopSettings")
        );

        return new DesktopSettingsDbContext(optionsBuilder.Options);
    }
}
