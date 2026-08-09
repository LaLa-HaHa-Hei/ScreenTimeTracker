using ScreenTimeTracker.BuildingBlocks.Domain;

namespace ScreenTimeTracker.ScreenTime.Domain.UserSettings;

public class AppTrackingSettings : ValueObject
{
    // 记录使用数据
    public string IconDirectory { get; private set; }
    public TimeSpan MetadataStaleThreshold { get; private set; }
    public TimeSpan ActiveUsageSessionAutoSaveInterval { get; private set; }

    // 使用数据优化
    public TimeSpan MinValidUsageSessionDuration { get; private set; }
    public TimeSpan UsageSessionMergeTolerance { get; private set; }
    public TimeSpan UsageSessionOptimizationInterval { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AppTrackingSettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return IconDirectory;
        yield return MetadataStaleThreshold;
        yield return ActiveUsageSessionAutoSaveInterval;
        yield return MinValidUsageSessionDuration;
        yield return UsageSessionMergeTolerance;
        yield return UsageSessionOptimizationInterval;
    }

    public AppTrackingSettings(
        string iconDirectory,
        TimeSpan metadataStaleThreshold,
        TimeSpan activeUsageSessionAutoSaveInterval,
        TimeSpan minValidUsageSessionDuration,
        TimeSpan usageSessionMergeTolerance,
        TimeSpan usageSessionOptimizationInterval
    )
    {
        if (metadataStaleThreshold < TimeSpan.Zero)
            throw new ArgumentException(
                "Metadata stale threshold must be greater than or equal to zero.",
                nameof(metadataStaleThreshold)
            );
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
        MetadataStaleThreshold = metadataStaleThreshold;
        ActiveUsageSessionAutoSaveInterval = activeUsageSessionAutoSaveInterval;
        MinValidUsageSessionDuration = minValidUsageSessionDuration;
        UsageSessionMergeTolerance = usageSessionMergeTolerance;
        UsageSessionOptimizationInterval = usageSessionOptimizationInterval;
    }

    public static AppTrackingSettings CreateDefault() =>
        new(
            "./Data/AppIcons",
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromMinutes(10)
        );
}
