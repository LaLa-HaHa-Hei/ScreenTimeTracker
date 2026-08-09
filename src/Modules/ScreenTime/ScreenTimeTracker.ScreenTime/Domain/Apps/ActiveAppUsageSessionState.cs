namespace ScreenTimeTracker.ScreenTime.Domain.Apps;

public record ActiveAppUsageSessionState(Guid AppId, DateTimeOffset StartTime);
