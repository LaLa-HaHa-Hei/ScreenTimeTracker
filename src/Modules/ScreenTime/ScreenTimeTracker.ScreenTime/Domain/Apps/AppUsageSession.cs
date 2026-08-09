using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.Apps;

public class AppUsageSession : AggregateRoot
{
    public Guid AppId { get; private set; }
    public App? App { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public bool IsOptimized { get; private set; }

    private AppUsageSession(
        Guid id,
        Guid appId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        bool isOptimized
    )
        : base(id)
    {
        AppId = appId;
        StartTime = startTime;
        EndTime = endTime;
        IsOptimized = isOptimized;
    }

    public static AppUsageSession Create(
        Guid appId,
        DateTimeOffset startTime,
        DateTimeOffset endTime
    )
    {
        if (endTime <= startTime)
            throw new ArgumentException(
                "End time must be greater than start time.",
                nameof(endTime)
            );
        return new AppUsageSession(Guid.CreateVersion7(), appId, startTime, endTime, false);
    }

    public static AppUsageSession Import(
        Guid appId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        bool isOptimized = false
    )
    {
        if (endTime <= startTime)
            throw new ArgumentException(
                "End time must be greater than start time.",
                nameof(endTime)
            );
        return new AppUsageSession(Guid.CreateVersion7(), appId, startTime, endTime, isOptimized);
    }

    public void MarkAsIdle(Guid idleAppId) => AppId = idleAppId;

    public void UpdateEndTime(DateTimeOffset endTime)
    {
        if (endTime <= StartTime)
            throw new ArgumentException(
                "End time must be greater than start time.",
                nameof(endTime)
            );
        EndTime = endTime;
    }

    public void MarkAsOptimized() => IsOptimized = true;
}
