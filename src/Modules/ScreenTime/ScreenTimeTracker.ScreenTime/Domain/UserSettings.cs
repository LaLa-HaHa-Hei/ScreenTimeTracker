using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Domain;

public class UserSettings : AggregateRoot
{
    public static readonly Guid DefaultId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    // 记录使用数据
    public string AppIconDirectory { get; private set; }
    public TimeSpan AppMetadataStaleThreshold { get; private set; }
    public TimeSpan ActiveAppUsageSessionAutoSaveInterval { get; private set; }

    // 空闲检测
    public bool IsIdleDetectionEnabled { get; private set; }
    public TimeSpan IdleThreshold { get; private set; }
    public TimeSpan IdleDetectionPollingInterval { get; private set; }

    // 使用数据优化
    public TimeSpan MinValidAppUsageSessionDuration { get; private set; }
    public TimeSpan AppUsageSessionMergeTolerance { get; private set; }
    public TimeSpan AppUsageSessionOptimizationInterval { get; private set; }

    // 数据呈现
    public int DayCutoffHour { get; private set; }

    private UserSettings(
        Guid id,
        string appIconDirectory,
        TimeSpan appMetadataStaleThreshold,
        TimeSpan activeAppUsageSessionAutoSaveInterval,
        bool isIdleDetectionEnabled,
        TimeSpan idleThreshold,
        TimeSpan idleDetectionPollingInterval,
        TimeSpan minValidAppUsageSessionDuration,
        TimeSpan appUsageSessionMergeTolerance,
        TimeSpan appUsageSessionOptimizationInterval,
        int dayCutoffHour
    )
        : base(id)
    {
        AppIconDirectory = appIconDirectory;
        AppMetadataStaleThreshold = appMetadataStaleThreshold;
        ActiveAppUsageSessionAutoSaveInterval = activeAppUsageSessionAutoSaveInterval;
        IsIdleDetectionEnabled = isIdleDetectionEnabled;
        IdleThreshold = idleThreshold;
        IdleDetectionPollingInterval = idleDetectionPollingInterval;
        MinValidAppUsageSessionDuration = minValidAppUsageSessionDuration;
        AppUsageSessionMergeTolerance = appUsageSessionMergeTolerance;
        AppUsageSessionOptimizationInterval = appUsageSessionOptimizationInterval;
        DayCutoffHour = dayCutoffHour;
    }

    public static UserSettings CreateDefault() =>
        new(
            DefaultId,
            "./Data/AppIcons",
            TimeSpan.FromHours(24),
            TimeSpan.FromSeconds(15),
            false,
            TimeSpan.FromMinutes(10),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(6),
            TimeSpan.FromMinutes(10),
            5
        );

    public void Update(
        OptionalValue<string> appIconDirectory,
        OptionalValue<TimeSpan> appMetadataStaleThreshold,
        OptionalValue<TimeSpan> activeAppUsageSessionAutoSaveInterval,
        OptionalValue<bool> isIdleDetectionEnabled,
        OptionalValue<TimeSpan> idleThreshold,
        OptionalValue<TimeSpan> idleDetectionPollingInterval,
        OptionalValue<TimeSpan> minValidAppUsageSessionDuration,
        OptionalValue<TimeSpan> appUsageSessionMergeTolerance,
        OptionalValue<TimeSpan> appUsageSessionOptimizationInterval,
        OptionalValue<int> dayCutoffHour
    )
    {
        if (appMetadataStaleThreshold.HasValue && appMetadataStaleThreshold.Value < TimeSpan.Zero)
            throw new ArgumentException(
                "App metadata stale threshold must be greater than or equal to zero.",
                nameof(appMetadataStaleThreshold)
            );
        if (
            activeAppUsageSessionAutoSaveInterval.HasValue
            && activeAppUsageSessionAutoSaveInterval.Value <= TimeSpan.Zero
        )
            throw new ArgumentException(
                "Active app usage session auto save interval must be greater than zero.",
                nameof(activeAppUsageSessionAutoSaveInterval)
            );
        if (idleThreshold.HasValue && idleThreshold.Value <= TimeSpan.Zero)
            throw new ArgumentException(
                "Idle threshold must be greater than zero.",
                nameof(idleThreshold)
            );
        if (
            idleDetectionPollingInterval.HasValue
            && idleDetectionPollingInterval.Value <= TimeSpan.Zero
        )
            throw new ArgumentException(
                "Idle detection polling interval must be greater than zero.",
                nameof(idleDetectionPollingInterval)
            );
        if (
            minValidAppUsageSessionDuration.HasValue
            && minValidAppUsageSessionDuration.Value < TimeSpan.Zero
        )
            throw new ArgumentException(
                "Min valid app usage session duration must be greater than or equal to zero.",
                nameof(minValidAppUsageSessionDuration)
            );
        if (
            appUsageSessionMergeTolerance.HasValue
            && appUsageSessionMergeTolerance.Value < TimeSpan.Zero
        )
            throw new ArgumentException(
                "App usage session merge tolerance must be greater than or equal to zero.",
                nameof(appUsageSessionMergeTolerance)
            );
        if (
            appUsageSessionOptimizationInterval.HasValue
            && appUsageSessionOptimizationInterval.Value <= TimeSpan.Zero
        )
            throw new ArgumentException(
                "App usage session optimization interval must be greater than zero.",
                nameof(appUsageSessionOptimizationInterval)
            );
        if (dayCutoffHour.HasValue && (dayCutoffHour.Value < 0 || dayCutoffHour.Value > 23))
            throw new ArgumentException(
                "Day cutoff hour must be between 0 and 23.",
                nameof(dayCutoffHour)
            );

        if (appIconDirectory.HasValue)
            AppIconDirectory = appIconDirectory.Value;
        if (appMetadataStaleThreshold.HasValue)
            AppMetadataStaleThreshold = appMetadataStaleThreshold.Value;
        if (activeAppUsageSessionAutoSaveInterval.HasValue)
            ActiveAppUsageSessionAutoSaveInterval = activeAppUsageSessionAutoSaveInterval.Value;
        if (isIdleDetectionEnabled.HasValue)
            IsIdleDetectionEnabled = isIdleDetectionEnabled.Value;
        if (idleThreshold.HasValue)
            IdleThreshold = idleThreshold.Value;
        if (idleDetectionPollingInterval.HasValue)
            IdleDetectionPollingInterval = idleDetectionPollingInterval.Value;
        if (minValidAppUsageSessionDuration.HasValue)
            MinValidAppUsageSessionDuration = minValidAppUsageSessionDuration.Value;
        if (appUsageSessionMergeTolerance.HasValue)
            AppUsageSessionMergeTolerance = appUsageSessionMergeTolerance.Value;
        if (appUsageSessionOptimizationInterval.HasValue)
            AppUsageSessionOptimizationInterval = appUsageSessionOptimizationInterval.Value;
        if (dayCutoffHour.HasValue)
            DayCutoffHour = dayCutoffHour.Value;
    }
}
