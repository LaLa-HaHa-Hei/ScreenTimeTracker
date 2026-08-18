using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;

public record ActiveWebsiteUsageSessionState(
    Guid WebsiteId,
    DateTimeOffset StartTime,
    DateTimeOffset LastActiveAt
) : IValueObject;
