using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.UserSettings;

public record IdleDetectionSettings : IValueObject
{
    public bool IsEnabled { get; }
    public TimeSpan InactivityThreshold { get; }
    public TimeSpan PollingInterval { get; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public IdleDetectionSettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public IdleDetectionSettings(
        bool isIdleDetectionEnabled,
        TimeSpan idleThreshold,
        TimeSpan idleDetectionPollingInterval
    )
    {
        if (idleThreshold <= TimeSpan.Zero)
            throw new ArgumentException(
                "Idle threshold must be greater than zero.",
                nameof(idleThreshold)
            );
        if (idleDetectionPollingInterval <= TimeSpan.Zero)
            throw new ArgumentException(
                "Idle detection polling interval must be greater than zero.",
                nameof(idleDetectionPollingInterval)
            );

        IsEnabled = isIdleDetectionEnabled;
        InactivityThreshold = idleThreshold;
        PollingInterval = idleDetectionPollingInterval;
    }

    public static IdleDetectionSettings CreateDefault() =>
        new(true, TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(10));
}
