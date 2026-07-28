using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.DesktopSettings.Domain;

namespace ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence.Configurations
{
    public class LocalSettingsConfiguration : IEntityTypeConfiguration<LocalSettings>
    {
        public void Configure(EntityTypeBuilder<LocalSettings> builder)
        {
            builder.HasData(LocalSettings.CreateDefault());
        }
    }
}
