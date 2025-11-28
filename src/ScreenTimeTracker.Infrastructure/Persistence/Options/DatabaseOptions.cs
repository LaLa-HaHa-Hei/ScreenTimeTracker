namespace ScreenTimeTracker.Infrastructure.Persistence.Options
{
    public class DatabaseOptions
    {
        public static readonly string SectionName = "DatabaseSettings";
        public string DBFilePath { get; set; } = "./Data/ScreenTimeTracker.db";
    }
}