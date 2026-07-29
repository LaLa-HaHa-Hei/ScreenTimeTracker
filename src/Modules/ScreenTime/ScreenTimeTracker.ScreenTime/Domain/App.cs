using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Domain;

public class App : AggregateRoot
{
    public static readonly Guid IdleAppId = new("00000000-0000-0000-0000-000000000001");
    public static readonly string IdleAppProcessName = "Idle";
    public static readonly Guid UnknownAppId = new("00000000-0000-0000-0000-000000000002");
    public static readonly string UnknownAppProcessName = "Unknown";

    public string Name { get; private set; }
    public string Color { get; private set; }
    public string ProcessName { get; private set; }
    public bool AllowMetadataAutoUpdate { get; private set; }
    public DateTime MetadataLastUpdatedAt { get; private set; }
    public Guid AppCategoryId { get; private set; }
    public AppCategory? AppCategory { get; private set; }
    public string? ExecutablePath { get; private set; }
    public string? IconPath { get; private set; }
    public DateTime IconPathLastUpdatedAt { get; private set; }
    public bool IsSystem { get; private set; }

    [Obsolete("a", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public App() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private App(
        Guid id,
        string name,
        string color,
        string processName,
        bool allowMetadataAutoUpdate,
        DateTime metadataLastUpdatedAt,
        Guid appCategoryId,
        string? executablePath,
        string? iconPath,
        DateTime iconPathLastUpdatedAt,
        bool isSystem
    )
        : base(id)
    {
        Name = name;
        Color = color;
        ProcessName = processName;
        AllowMetadataAutoUpdate = allowMetadataAutoUpdate;
        MetadataLastUpdatedAt = metadataLastUpdatedAt;
        AppCategoryId = appCategoryId;
        ExecutablePath = executablePath;
        IconPath = iconPath;
        IconPathLastUpdatedAt = iconPathLastUpdatedAt;
        IsSystem = isSystem;
    }

    public static App RegisterDiscovered(
        DateTime discoveredAt,
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
        bool isAutoUpdateEnabled,
        DateTime lastAutoUpdated,
        Guid appCategoryId,
        string? executablePath,
        string? iconPath,
        DateTime iconPathLastUpdatedAt
    ) =>
        new(
            Guid.CreateVersion7(),
            name,
            color,
            processName,
            isAutoUpdateEnabled,
            lastAutoUpdated,
            appCategoryId,
            executablePath,
            iconPath,
            iconPathLastUpdatedAt,
            false
        );

    public static App CreateIdleApp() =>
        new(
            IdleAppId,
            "Idle",
            "#C7C7CC",
            IdleAppProcessName,
            false,
            DateTime.MinValue,
            AppCategory.UncategorizedId,
            null,
            null,
            DateTime.MinValue,
            true
        );

    public static App CreateUnknownApp() =>
        new(
            UnknownAppId,
            "Unknown",
            "#636366",
            UnknownAppProcessName,
            false,
            DateTime.MinValue,
            AppCategory.UncategorizedId,
            null,
            null,
            DateTime.MinValue,
            true
        );

    public void Update(
        OptionalValue<string> name = default,
        OptionalValue<string> color = default,
        OptionalValue<bool> allowMetadataAutoUpdate = default,
        OptionalValue<Guid> appCategoryId = default,
        OptionalValue<string?> iconPath = default,
        DateTime? iconPathUpdatedAt = null
    )
    {
        if (iconPath.HasValue && iconPathUpdatedAt is null)
            throw new ArgumentException(
                "Icon path must be updated with a valid icon path last updated time."
            );

        if (name.HasValue && name.Value != Name)
            Name = name.Value;
        if (color.HasValue && color.Value != Color)
            Color = color.Value;
        if (
            allowMetadataAutoUpdate.HasValue
            && allowMetadataAutoUpdate.Value != AllowMetadataAutoUpdate
        )
            AllowMetadataAutoUpdate = allowMetadataAutoUpdate.Value;
        if (appCategoryId.HasValue && appCategoryId.Value != AppCategoryId)
            AppCategoryId = appCategoryId.Value;
        if (iconPath.HasValue && iconPath.Value != IconPath)
        {
            IconPath = iconPath.Value;
            IconPathLastUpdatedAt = iconPathUpdatedAt!.Value;
        }
    }

    public void UpdateMetadata(DateTime updatedAt, string? executablePath, string? iconPath)
    {
        if (!AllowMetadataAutoUpdate)
            throw new InvalidOperationException(
                $"Cannot update system details for App '{Name}' because auto-update is disabled."
            );

        ExecutablePath = executablePath;
        IconPath = iconPath;
        IconPathLastUpdatedAt = updatedAt;
        MetadataLastUpdatedAt = updatedAt;
    }

    public bool NeedsMetadataUpdate(DateTime now, TimeSpan threshold) =>
        AllowMetadataAutoUpdate && (now - MetadataLastUpdatedAt) >= threshold;

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
