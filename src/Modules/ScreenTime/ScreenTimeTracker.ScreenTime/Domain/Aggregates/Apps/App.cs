using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.Apps;

public class App : AggregateRoot
{
    public static readonly Guid IdleAppId = new("00000000-0000-0000-0000-000000000001");
    public static readonly string IdleAppProcessName = "Idle";
    public static readonly Guid UnknownAppId = new("00000000-0000-0000-0000-000000000002");
    public static readonly string UnknownAppProcessName = "Unknown";

    public string Name { get; private set; }
    public string Color { get; private set; }
    public string ProcessName { get; private set; }
    public bool AllowMetadataAutoRefresh { get; private set; }
    public DateTimeOffset MetadataLastRefreshedAt { get; private set; }
    public Guid AppCategoryId { get; private set; }
    public string? ExecutablePath { get; private set; }
    public string? IconPath { get; private set; }
    public DateTimeOffset IconPathLastUpdatedAt { get; private set; }
    public bool IsSystem { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public App() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private App(
        Guid id,
        string name,
        string color,
        string processName,
        bool allowMetadataAutoRefresh,
        DateTimeOffset metadataLastRefreshedAt,
        Guid appCategoryId,
        string? executablePath,
        string? iconPath,
        DateTimeOffset iconPathLastUpdatedAt,
        bool isSystem
    )
        : base(id)
    {
        Name = name;
        Color = color;
        ProcessName = processName;
        AllowMetadataAutoRefresh = allowMetadataAutoRefresh;
        MetadataLastRefreshedAt = metadataLastRefreshedAt;
        AppCategoryId = appCategoryId;
        ExecutablePath = executablePath;
        IconPath = iconPath;
        IconPathLastUpdatedAt = iconPathLastUpdatedAt;
        IsSystem = isSystem;
    }

    public static App CreateDiscovered(
        DateTimeOffset discoveredAt,
        string name,
        string processName,
        string? executablePath = null,
        string? iconPath = null
    ) =>
        new(
            Guid.CreateVersion7(),
            name,
            GenerateColor(),
            processName,
            true,
            discoveredAt,
            AppCategory.UncategorizedId,
            executablePath,
            iconPath,
            discoveredAt,
            false
        );

    public static App Import(
        string name,
        string color,
        string processName,
        bool allowMetadataAutoRefresh,
        Guid appCategoryId,
        string? iconPath
    ) =>
        new(
            Guid.CreateVersion7(),
            name,
            color,
            processName,
            allowMetadataAutoRefresh,
            DateTimeOffset.MinValue,
            appCategoryId,
            null,
            iconPath,
            DateTimeOffset.MinValue,
            false
        );

    public static App CreateIdleApp() =>
        new(
            IdleAppId,
            "Idle",
            "#C7C7CC",
            IdleAppProcessName,
            false,
            DateTimeOffset.MinValue,
            AppCategory.UncategorizedId,
            null,
            null,
            DateTimeOffset.MinValue,
            true
        );

    public static App CreateUnknownApp() =>
        new(
            UnknownAppId,
            "Unknown",
            "#636366",
            UnknownAppProcessName,
            false,
            DateTimeOffset.MinValue,
            AppCategory.UncategorizedId,
            null,
            null,
            DateTimeOffset.MinValue,
            true
        );

    public void Update(
        OptionalValue<string> name = default,
        OptionalValue<string> color = default,
        OptionalValue<bool> allowMetadataAutoRefresh = default,
        OptionalValue<Guid> appCategoryId = default,
        OptionalValue<string?> iconPath = default,
        DateTimeOffset? iconPathUpdatedAt = null
    )
    {
        if (iconPath.HasValue && iconPathUpdatedAt is null)
            throw new ArgumentException(
                "Icon path must be updated with a valid icon path last updated time."
            );

        if (name.HasValue)
            Name = name.Value;
        if (color.HasValue)
            Color = color.Value;
        if (allowMetadataAutoRefresh.HasValue)
            AllowMetadataAutoRefresh = allowMetadataAutoRefresh.Value;
        if (appCategoryId.HasValue)
            AppCategoryId = appCategoryId.Value;
        if (iconPath.HasValue)
        {
            IconPath = iconPath.Value;
            IconPathLastUpdatedAt = iconPathUpdatedAt!.Value;
        }
    }

    public void RefreshMetadata(
        DateTimeOffset refreshedAt,
        string? executablePath,
        string? iconPath
    )
    {
        if (!AllowMetadataAutoRefresh)
            throw new InvalidOperationException(
                $"Cannot refresh system details for App '{Name}' because auto-refresh is disabled."
            );

        ExecutablePath = executablePath;
        IconPath = iconPath;
        IconPathLastUpdatedAt = refreshedAt;
        MetadataLastRefreshedAt = refreshedAt;
    }

    public bool NeedsMetadataRefresh(DateTimeOffset now, TimeSpan threshold) =>
        AllowMetadataAutoRefresh && (now - MetadataLastRefreshedAt) >= threshold;

    public static string GenerateColor()
    {
        int h = Random.Shared.Next(360);
        int s = Random.Shared.Next(50, 80);
        int l = Random.Shared.Next(50, 80);
        return HslToHex(h, s, l);
    }

    private static string HslToHex(double h, double s, double l)
    {
        h %= 360;
        s /= 100.0;
        l /= 100.0;

        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h / 60.0) % 2 - 1));
        double m = l - c / 2;

        double r1 = 0,
            g1 = 0,
            b1 = 0;

        if (h < 60)
            (r1, g1, b1) = (c, x, 0);
        else if (h < 120)
            (r1, g1, b1) = (x, c, 0);
        else if (h < 180)
            (r1, g1, b1) = (0, c, x);
        else if (h < 240)
            (r1, g1, b1) = (0, x, c);
        else if (h < 300)
            (r1, g1, b1) = (x, 0, c);
        else
            (r1, g1, b1) = (c, 0, x);

        int r = (int)Math.Round((r1 + m) * 255);
        int g = (int)Math.Round((g1 + m) * 255);
        int b = (int)Math.Round((b1 + m) * 255);

        return $"#{r:X2}{g:X2}{b:X2}";
    }
}
