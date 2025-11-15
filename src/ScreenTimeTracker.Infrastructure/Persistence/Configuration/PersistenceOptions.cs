namespace ScreenTimeTracker.Infrastructure.Persistence.Configuration
{
    public class PersistenceOptions
    {
        public static readonly string SectionName = "PersistenceSettings";
        public string DBFilePath { get; set; } = "./ScreenTimeTracker.db";
    }
}