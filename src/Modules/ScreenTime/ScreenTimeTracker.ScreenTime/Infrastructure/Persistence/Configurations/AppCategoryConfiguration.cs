using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.Persistence.Configurations
{
    public class AppCategoryConfiguration : IEntityTypeConfiguration<AppCategory>
    {
        public void Configure(EntityTypeBuilder<AppCategory> builder)
        {
            builder.HasData(AppCategory.CreateUncategorized());

            builder.HasIndex(x => x.Name).IsUnique();
        }
    }
}
