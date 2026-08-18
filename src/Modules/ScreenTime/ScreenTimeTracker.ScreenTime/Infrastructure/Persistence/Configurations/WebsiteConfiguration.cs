using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteCategories;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class TrackedWebsiteConfiguration : IEntityTypeConfiguration<Website>
    {
        public void Configure(EntityTypeBuilder<Website> builder)
        {
            builder
                .HasOne<WebsiteCategory>()
                .WithMany()
                .HasForeignKey(a => a.WebsiteCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // 如果存在 Website，阻止删除 Category。

            builder.HasIndex(x => x.Host).IsUnique();
            builder.HasIndex(x => x.WebsiteCategoryId);
        }
    }
}
