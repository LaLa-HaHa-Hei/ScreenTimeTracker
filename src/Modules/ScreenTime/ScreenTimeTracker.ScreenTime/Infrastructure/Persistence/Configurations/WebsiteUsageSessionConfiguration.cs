using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.Websites;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class WebsiteUsageSessionConfiguration : IEntityTypeConfiguration<WebsiteUsageSession>
    {
        public void Configure(EntityTypeBuilder<WebsiteUsageSession> builder)
        {
            // 多个 WebsiteUsageSession 对应一个 Website
            builder
                .HasOne(x => x.Website)
                .WithMany()
                .HasForeignKey(x => x.WebsiteId)
                .OnDelete(DeleteBehavior.Cascade); // 删 Website 时连带删 WebsiteUsageSession

            builder.HasIndex(x => x.StartTime);
            builder.HasIndex(x => x.EndTime);
            builder.HasIndex(x => new { x.WebsiteId, x.EndTime }); // 因为都是两集合交集查询，通常 EndTime 比 StartTime 能筛选掉更多数据
        }
    }
}
