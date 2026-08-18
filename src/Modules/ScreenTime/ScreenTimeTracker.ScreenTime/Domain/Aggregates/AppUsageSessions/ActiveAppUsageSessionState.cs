using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;

public record ActiveAppUsageSessionState(Guid AppId, DateTimeOffset StartTime) : IValueObject;
