using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Configurations
{
    public class HourlySummaryEntityConfiguration : IEntityTypeConfiguration<HourlySummaryEntity>
    {
        public void Configure(EntityTypeBuilder<HourlySummaryEntity> builder)
        {
            builder.HasKey(h => new { h.ProcessInfoEntityId, h.Hour });

            builder.HasOne(e => e.ProcessInfoEntity)
                   .WithMany(p => p.HourlySummaries)
                   .HasForeignKey(e => e.ProcessInfoEntityId)
                    .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
