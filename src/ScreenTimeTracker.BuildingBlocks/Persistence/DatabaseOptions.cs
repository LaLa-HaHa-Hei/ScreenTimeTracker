namespace ScreenTimeTracker.BuildingBlocks.Persistence;

public class DatabaseOptions
{
    public static readonly string SectionName = "DatabaseOptions";
    public string DBFilePath { get; set; } = "./Data/ScreenTimeTracker.db";
}
