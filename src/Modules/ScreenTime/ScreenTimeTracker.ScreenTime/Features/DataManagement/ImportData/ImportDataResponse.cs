namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ImportData;

public record ImportDataResponse(
    long NewAppCategories,
    long NewApps,
    long ImportedSessions,
    long SkippedSessions
);
