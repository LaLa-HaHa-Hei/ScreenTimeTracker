using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.Infrastructure.Persistence.Models;

namespace ScreenTimeTracker.Infrastructure.Persistence.Configurations
{
    public class ProcessInfoEntityConfiguration : IEntityTypeConfiguration<ProcessInfoEntity>
    {
        public void Configure(EntityTypeBuilder<ProcessInfoEntity> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired();
            builder.HasIndex(p => p.Name).IsUnique();
        }
    }
}
