using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.UserSettings;

public class WebsiteTrackingSettings : ValueObject
{
    // 记录使用数据
    public string IconDirectory { get; private set; }
    public TimeSpan ActiveUsageSessionAutoSaveInterval { get; private set; }

    // 使用数据优化
    public TimeSpan MinValidUsageSessionDuration { get; private set; }
    public TimeSpan UsageSessionMergeTolerance { get; private set; }
    public TimeSpan UsageSessionOptimizationInterval { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public WebsiteTrackingSettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IconDirectory;
        yield return ActiveUsageSessionAutoSaveInterval;
        yield return MinValidUsageSessionDuration;
        yield return UsageSessionMergeTolerance;
        yield return UsageSessionOptimizationInterval;
    }

    public WebsiteTrackingSettings(
        string iconDirectory,
        TimeSpan activeUsageSessionAutoSaveInterval,
        TimeSpan minValidUsageSessionDuration,
        TimeSpan usageSessionMergeTolerance,
        TimeSpan usageSessionOptimizationInterval
    )
    {
        if (activeUsageSessionAutoSaveInterval <= TimeSpan.Zero)
            throw new ArgumentException(
                "Active usage session auto save interval must be greater than zero.",
                nameof(activeUsageSessionAutoSaveInterval)
            );
        if (minValidUsageSessionDuration < TimeSpan.Zero)
            throw new ArgumentException(
                "Min valid usage session duration must be greater than or equal to zero.",
                nameof(minValidUsageSessionDuration)
            );
        if (usageSessionMergeTolerance < TimeSpan.Zero)
            throw new ArgumentException(
                "Usage session merge tolerance must be greater than or equal to zero.",
                nameof(usageSessionMergeTolerance)
            );
        if (usageSessionOptimizationInterval <= TimeSpan.Zero)
            throw new ArgumentException(
                "Usage session optimization interval must be greater than zero.",
                nameof(usageSessionOptimizationInterval)
            );

        IconDirectory = iconDirectory;
        ActiveUsageSessionAutoSaveInterval = activeUsageSessionAutoSaveInterval;
        MinValidUsageSessionDuration = minValidUsageSessionDuration;
        UsageSessionMergeTolerance = usageSessionMergeTolerance;
        UsageSessionOptimizationInterval = usageSessionOptimizationInterval;
    }

    public static WebsiteTrackingSettings CreateDefault() =>
        new(
            "./Data/WebsiteIcons",
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromMinutes(10)
        );
}
