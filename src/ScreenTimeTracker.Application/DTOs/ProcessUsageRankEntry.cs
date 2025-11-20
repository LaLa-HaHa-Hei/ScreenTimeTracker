namespace ScreenTimeTracker.Application.DTOs
{
    public record ProcessUsageRankEntry(
        Guid ProcessId,
        string ProcessName,
        string? ProcessAlias,
        string? ProcessIconPath,
        long TotalDuration,
        int Percentage
    );
}
