namespace ScreenTimeTracker.Infrastructure.Persistence.Options
{
    public class UserConfigStorageOptions
    {
        public static readonly string SectionName = "UserConfigStorageSettings";
        public string FilePath { get; set; } = "./Data/UserConfig.json";
    }
}