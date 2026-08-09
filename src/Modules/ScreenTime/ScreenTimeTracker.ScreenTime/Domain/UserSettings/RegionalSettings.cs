using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.UserSettings;

public class RegionalSettings : ValueObject
{
    public string TimeZoneId { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public RegionalSettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TimeZoneId;
    }

    public RegionalSettings(string timeZoneId)
    {
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out _))
            throw new ArgumentException($"Invalid Time Zone: {timeZoneId}");

        TimeZoneId = timeZoneId;
    }

    public static RegionalSettings CreateDefault() => new("America/New_York");
}
