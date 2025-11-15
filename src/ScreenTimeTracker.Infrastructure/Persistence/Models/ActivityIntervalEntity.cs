namespace ScreenTimeTracker.Infrastructure.Persistence.Models
{
    public class ActivityIntervalEntity
    {
        public required Guid Id { get; set; }
        public required DateTime Timestamp { get; set; }
        public required Guid ProcessInfoEntityId { get; set; }
        public required ProcessInfoEntity ProcessInfoEntity { get; set; }
        public required int DurationMilliseconds { get; set; }
    }
}
