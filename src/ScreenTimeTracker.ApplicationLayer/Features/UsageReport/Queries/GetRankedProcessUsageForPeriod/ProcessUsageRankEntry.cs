namespace ScreenTimeTracker.ApplicationLayer.Features.UsageReport.Queries.GetRankedProcessUsageForPeriod;

public record ProcessUsageRankEntry(
    Guid ProcessId,
    string ProcessName,
    string? ProcessAlias,
    string? ProcessIconPath,
    long TotalDuration,
    int Percentage
);

