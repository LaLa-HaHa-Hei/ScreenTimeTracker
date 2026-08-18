using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;

public class AppUsageSession : AggregateRoot
{
    public Guid AppId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public TimeRange UsagePeriod => new(StartTime, EndTime);

    public bool IsOptimized { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AppUsageSession() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private AppUsageSession(Guid id, Guid appId, TimeRange usagePeriod, bool isOptimized)
        : base(id)
    {
        AppId = appId;
        StartTime = usagePeriod.Start;
        EndTime = usagePeriod.End;
        IsOptimized = isOptimized;
    }

    public static AppUsageSession Create(Guid appId, TimeRange usagePeriod) =>
        new(Guid.CreateVersion7(), appId, usagePeriod, false);

    public static AppUsageSession Import(Guid appId, TimeRange usagePeriod) =>
        new(Guid.CreateVersion7(), appId, usagePeriod, false);

    public void MarkAsIdle(Guid idleAppId) => AppId = idleAppId;

    public void UpdateEndTime(DateTimeOffset endTime)
    {
        EndTime = endTime;
    }

    public void MarkAsOptimized() => IsOptimized = true;
}
