using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Configurations
{
    public class ActivityIntervalEntityConfiguration : IEntityTypeConfiguration<ActivityIntervalEntity>
    {
        public void Configure(EntityTypeBuilder<ActivityIntervalEntity> builder)
        {
            builder.HasKey(a => a.Id);
            builder.HasIndex(a => a.Timestamp);
            builder.HasOne(a => a.ProcessInfoEntity)
                   .WithMany(p => p.ActivityIntervals)
                   .HasForeignKey(a => a.ProcessInfoEntityId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
