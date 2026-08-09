namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ExportData;

public record ExportDataResponse(
    ExportDataResponse.App[] Apps,
    ExportDataResponse.AppCategory[] AppCategories,
    ExportDataResponse.AppUsageSession[] AppUsageSessions,
    ExportDataResponse.Website[] Websites,
    ExportDataResponse.WebsiteCategory[] WebsiteCategories,
    ExportDataResponse.WebsiteUsageSession[] WebsiteUsageSessions
)
{
    public int Version { get; init; } = 4;

    public record Icon(string Extension, byte[] Data);

    public record App(
        string Name,
        string Color,
        string ProcessName,
        bool AllowMetadataAutoRefresh,
        string AppCategoryName,
        Icon? Icon
    );

    public record AppCategory(string Name, string Color, Icon? Icon);

    public record AppUsageSession(
        string AppProcessName,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime
    );

    public record Website(
        string Name,
        string Color,
        string Host,
        bool AllowMetadataAutoRefresh,
        string WebsiteCategoryName,
        Icon? Icon
    );

    public record WebsiteCategory(string Name, string Color, Icon? Icon);

    public record WebsiteUsageSession(
        string WebsiteHost,
        DateTimeOffset StartTime,
        DateTimeOffset EndTime
    );
};
