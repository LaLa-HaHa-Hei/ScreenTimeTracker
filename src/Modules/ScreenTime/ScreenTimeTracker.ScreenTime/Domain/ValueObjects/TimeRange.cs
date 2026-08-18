using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.ValueObjects;

public readonly record struct TimeRange : IValueObject
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    public TimeSpan Duration => End - Start;

    public TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start)
            throw new ArgumentException(
                "End time must be greater than or equal to Start time.",
                nameof(end)
            );

        Start = start;
        End = end;
    }

    public bool Overlaps(TimeRange other) => other.Start < End && other.End > Start;

    public TimeSpan OverlapDuration(TimeRange other)
    {
        var overlapStart = other.Start > Start ? other.Start : Start;
        var overlapEnd = other.End < End ? other.End : End;

        return overlapEnd > overlapStart ? overlapEnd - overlapStart : TimeSpan.Zero;
    }

    public TimeRange? Intersect(TimeRange other)
    {
        var clampedStart = Start < other.Start ? other.Start : Start;
        var clampedEnd = End > other.End ? other.End : End;

        return clampedStart < clampedEnd ? new TimeRange(clampedStart, clampedEnd) : null;
    }
}
