namespace Shared.Constants
{
    public static class FilePaths
    {
        public static readonly string DataDirPath = "Data";
        public static readonly string IconDirPath = Path.Combine(DataDirPath, "Icons");
        public static readonly string DbFilePath = Path.Combine(DataDirPath, "Data.db");
    }
}
