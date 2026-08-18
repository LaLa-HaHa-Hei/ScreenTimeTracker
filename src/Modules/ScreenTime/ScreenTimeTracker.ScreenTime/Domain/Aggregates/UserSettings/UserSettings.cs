using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.UserSettings;

public class UserSettings : AggregateRoot
{
    public static readonly Guid DefaultId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public AppTrackingSettings AppTracking { get; private set; }
    public WebsiteTrackingSettings WebsiteTracking { get; private set; }
    public IdleDetectionSettings IdleDetection { get; private set; }
    public TimeBoundarySettings TimeBoundary { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public UserSettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private UserSettings(
        Guid id,
        AppTrackingSettings appTracking,
        WebsiteTrackingSettings websiteTracking,
        IdleDetectionSettings idleDetection,
        TimeBoundarySettings timeBoundary
    )
        : base(id)
    {
        AppTracking = appTracking;
        WebsiteTracking = websiteTracking;
        IdleDetection = idleDetection;
        TimeBoundary = timeBoundary;
    }

    public static UserSettings CreateDefault() =>
        new(
            DefaultId,
            AppTrackingSettings.CreateDefault(),
            WebsiteTrackingSettings.CreateDefault(),
            IdleDetectionSettings.CreateDefault(),
            TimeBoundarySettings.CreateDefault()
        );

    public void Update(
        OptionalValue<AppTrackingSettings> appTracking = default,
        OptionalValue<WebsiteTrackingSettings> websiteTracking = default,
        OptionalValue<IdleDetectionSettings> idleDetection = default,
        OptionalValue<TimeBoundarySettings> timeBoundary = default
    )
    {
        if (appTracking.HasValue)
            AppTracking = appTracking.Value;
        if (websiteTracking.HasValue)
            WebsiteTracking = websiteTracking.Value;
        if (idleDetection.HasValue)
            IdleDetection = idleDetection.Value;
        if (timeBoundary.HasValue)
            TimeBoundary = timeBoundary.Value;
    }
}
