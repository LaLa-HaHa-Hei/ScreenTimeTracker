using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteCategories;

namespace ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;

public class Website : AggregateRoot
{
    public string Name { get; private set; }
    public string Color { get; private set; }
    public string Host { get; private set; }
    public bool AllowMetadataAutoRefresh { get; private set; }
    public DateTimeOffset MetadataLastRefreshedAt { get; private set; }
    public Guid WebsiteCategoryId { get; private set; }
    public string? IconPath { get; private set; }
    public DateTimeOffset IconPathLastUpdatedAt { get; private set; }
    public bool IsSystem { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Website() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private Website(
        Guid id,
        string name,
        string color,
        string host,
        bool allowMetadataAutoRefresh,
        DateTimeOffset metadataLastRefreshedAt,
        Guid websiteCategoryId,
        string? iconPath,
        DateTimeOffset iconPathLastUpdatedAt,
        bool isSystem
    )
        : base(id)
    {
        Name = name;
        Color = color;
        Host = host;
        AllowMetadataAutoRefresh = allowMetadataAutoRefresh;
        MetadataLastRefreshedAt = metadataLastRefreshedAt;
        WebsiteCategoryId = websiteCategoryId;
        IconPath = iconPath;
        IconPathLastUpdatedAt = iconPathLastUpdatedAt;
        IsSystem = isSystem;
    }

    public static Website CreateDiscovered(
        DateTimeOffset discoveredAt,
        string name,
        string host,
        string? iconPath = null
    ) =>
        new(
            Guid.CreateVersion7(),
            name,
            GenerateColor(),
            host,
            true,
            discoveredAt,
            WebsiteCategory.UncategorizedId,
            iconPath,
            discoveredAt,
            false
        );

    public static Website Import(
        string name,
        string color,
        string host,
        bool allowMetadataAutoRefresh,
        Guid websiteCategoryId,
        string? iconPath
    ) =>
        new(
            Guid.CreateVersion7(),
            name,
            color,
            host,
            allowMetadataAutoRefresh,
            DateTimeOffset.MinValue,
            websiteCategoryId,
            iconPath,
            DateTimeOffset.MinValue,
            false
        );

    public void Update(
        OptionalValue<string> name = default,
        OptionalValue<string> color = default,
        OptionalValue<bool> allowMetadataAutoRefresh = default,
        OptionalValue<Guid> websiteCategoryId = default,
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
        if (websiteCategoryId.HasValue)
            WebsiteCategoryId = websiteCategoryId.Value;
        if (iconPath.HasValue)
        {
            IconPath = iconPath.Value;
            IconPathLastUpdatedAt = iconPathUpdatedAt!.Value;
        }
    }

    public void RefreshMetadata(DateTimeOffset refreshedAt, string? iconPath)
    {
        if (!AllowMetadataAutoRefresh)
            throw new InvalidOperationException(
                $"Cannot refresh system details for Website '{Name}' because auto-refresh is disabled."
            );

        IconPath = iconPath;
        MetadataLastRefreshedAt = refreshedAt;
        IconPathLastUpdatedAt = refreshedAt;
    }

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
