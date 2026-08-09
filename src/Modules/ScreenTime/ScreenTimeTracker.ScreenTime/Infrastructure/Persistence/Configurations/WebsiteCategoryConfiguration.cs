using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.Websites;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class WebsiteCategoryConfiguration : IEntityTypeConfiguration<WebsiteCategory>
    {
        public void Configure(EntityTypeBuilder<WebsiteCategory> builder)
        {
            builder.HasData(WebsiteCategory.CreateUncategorized());

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
