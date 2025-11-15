namespace ScreenTimeTracker.Infrastructure.Persistence.Models
{
    public class HourlySummaryEntity
    {
        public required Guid ProcessInfoEntityId { get; set; }
        public required ProcessInfoEntity ProcessInfoEntity { get; set; }
        public required DateTime Hour { get; set; }
        public required int TotalDurationMilliseconds { get; set; }
    }
}
