using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.Websites;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class TrackedWebsiteConfiguration : IEntityTypeConfiguration<Website>
    {
        public void Configure(EntityTypeBuilder<Website> builder)
        {
            builder
                .HasOne(a => a.Category)
                .WithMany()
                .HasForeignKey(a => a.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); //阻止删除Category，如果存在Website。

            builder.HasIndex(x => x.Host).IsUnique();
            builder.HasIndex(x => x.CategoryId);
        }
    }
}
