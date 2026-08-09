namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ImportData;

public record ImportDataResponse(
    long NewApps,
    long NewAppCategories,
    long NewWebsites,
    long NewWebsiteCategories,
    long ImportedAppUsageSessions,
    long SkippedAppUsageSessions,
    long ImportedWebsiteUsageSessions,
    long SkippedWebsiteUsageSessions
);
