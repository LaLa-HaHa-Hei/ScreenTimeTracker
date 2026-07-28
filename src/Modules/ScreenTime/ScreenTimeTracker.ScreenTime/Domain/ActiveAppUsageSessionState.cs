namespace ScreenTimeTracker.ScreenTime.Domain;

public record ActiveAppUsageSessionState(Guid AppId, DateTime StartTime);
