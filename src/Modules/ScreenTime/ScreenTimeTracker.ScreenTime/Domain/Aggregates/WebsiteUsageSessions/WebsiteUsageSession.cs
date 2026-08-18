using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;

public class WebsiteUsageSession : AggregateRoot
{
    public Guid WebsiteId { get; private set; }

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public TimeRange UsagePeriod => new(StartTime, EndTime);

    public bool IsOptimized { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public WebsiteUsageSession() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private WebsiteUsageSession(Guid id, Guid websiteId, TimeRange usagePeriod, bool isOptimized)
        : base(id)
    {
        WebsiteId = websiteId;
        StartTime = usagePeriod.Start;
        EndTime = usagePeriod.End;
        IsOptimized = isOptimized;
    }

    public static WebsiteUsageSession Create(Guid websiteId, TimeRange usagePeriod) =>
        new(Guid.CreateVersion7(), websiteId, usagePeriod, false);

    public static WebsiteUsageSession Import(Guid websiteId, TimeRange usagePeriod) =>
        new(Guid.CreateVersion7(), websiteId, usagePeriod, false);

    public void UpdateEndTime(DateTimeOffset endTime)
    {
        EndTime = endTime;
    }

    public void MarkAsOptimized() => IsOptimized = true;
}
