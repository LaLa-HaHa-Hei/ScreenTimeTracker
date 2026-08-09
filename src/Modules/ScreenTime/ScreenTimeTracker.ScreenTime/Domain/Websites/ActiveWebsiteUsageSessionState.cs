namespace ScreenTimeTracker.ScreenTime.Domain.Websites;

public record ActiveWebsiteUsageSessionState(
    Guid WebsiteId,
    DateTimeOffset StartTime,
    DateTimeOffset LastActiveAt
);
