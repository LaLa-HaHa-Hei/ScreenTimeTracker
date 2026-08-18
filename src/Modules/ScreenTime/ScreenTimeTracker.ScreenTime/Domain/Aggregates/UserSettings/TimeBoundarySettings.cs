using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.UserSettings;

public record TimeBoundarySettings : IValueObject
{
    public int DayCutoffHour { get; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public TimeBoundarySettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public TimeBoundarySettings(int dayCutoffHour)
    {
        if (dayCutoffHour < 0 || dayCutoffHour > 23)
            throw new ArgumentException(
                "Day cutoff hour must be between 0 and 23.",
                nameof(dayCutoffHour)
            );

        DayCutoffHour = dayCutoffHour;
    }

    public static TimeBoundarySettings CreateDefault() => new(4);
}
