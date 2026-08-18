using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Apps;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class TrackedAppConfiguration : IEntityTypeConfiguration<App>
    {
        public void Configure(EntityTypeBuilder<App> builder)
        {
            builder.HasData(App.CreateUnknownApp());
            builder.HasData(App.CreateIdleApp());

            builder
                .HasOne<AppCategory>()
                .WithMany()
                .HasForeignKey(a => a.AppCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // 如果存在 App，阻止删除 AppCategory。

            builder.HasIndex(x => x.ProcessName).IsUnique();
            builder.HasIndex(x => x.AppCategoryId);
        }
    }
}
