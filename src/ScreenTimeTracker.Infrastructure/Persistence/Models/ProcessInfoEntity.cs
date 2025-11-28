namespace ScreenTimeTracker.Infrastructure.Persistence.Models
{
    public class ProcessInfoEntity
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Alias { get; set; }
        public bool AutoUpdate { get; set; }
        public DateTime LastAutoUpdated { get; set; }
        public string? ExecutablePath { get; set; }
        public string? IconPath { get; set; }
        public string? Description { get; set; }

        public ICollection<HourlySummaryEntity> HourlySummaries { get; set; } = [];
        public ICollection<ActivityIntervalEntity> ActivityIntervals { get; set; } = [];
    }
}
