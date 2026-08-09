using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.Websites;

public class WebsiteUsageSession : AggregateRoot
{
    public Guid WebsiteId { get; private set; }
    public Website? Website { get; private set; }
    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public bool IsOptimized { get; private set; }

    private WebsiteUsageSession(
        Guid id,
        Guid websiteId,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        bool isOptimized
    )
        : base(id)
    {
        WebsiteId = websiteId;
        StartTime = startTime;
        EndTime = endTime;
        IsOptimized = isOptimized;
    }

    public static WebsiteUsageSession Create(
        Guid websiteId,
        DateTimeOffset startTime,
        DateTimeOffset endTime
    )
    {
        if (endTime <= startTime)
            throw new ArgumentException(
                "End time must be greater than start time.",
                nameof(endTime)
            );
        return new WebsiteUsageSession(Guid.CreateVersion7(), websiteId, startTime, endTime, false);
    }

    public static WebsiteUsageSession Import(
        Guid WebsiteId,
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
        return new WebsiteUsageSession(
            Guid.CreateVersion7(),
            WebsiteId,
            startTime,
            endTime,
            isOptimized
        );
    }

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
