using Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Data
{
    public class ScreenTimeContext(DbContextOptions<ScreenTimeContext> options) : DbContext(options)
    {
        public DbSet<ProcessInfo> ProcessInfos { get; set; }
        public DbSet<HourlyUsage> HourlyUsages { get; set; }
        public DbSet<DailyUsage> DailyUsages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<HourlyUsage>()
                .HasKey(x => new { x.Date, x.Hour, x.ProcessName });
            modelBuilder.Entity<DailyUsage>()
                .HasKey(x => new { x.Date, x.ProcessName });
        }
    }
}
