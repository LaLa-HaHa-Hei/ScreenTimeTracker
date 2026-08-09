using ScreenTimeTracker.BuildingBlocks.Domain;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Domain.Apps;

public class AppCategory : AggregateRoot
{
    public static readonly Guid UncategorizedId = new("00000000-0000-0000-0000-000000000001");
    public string Name { get; private set; }
    public string Color { get; private set; }
    public string? IconPath { get; private set; }
    public DateTimeOffset IconPathLastUpdatedAt { get; private set; }
    public bool IsSystem { get; private set; }

    [Obsolete("This constructor is reserved for framework use only. Do not call directly.", true)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public AppCategory() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private AppCategory(
        Guid id,
        string name,
        string color,
        string? iconPath,
        DateTimeOffset iconPathLastUpdatedAt,
        bool isSystem
    )
        : base(id)
    {
        Name = name;
        Color = color;
        IconPath = iconPath;
        IconPathLastUpdatedAt = iconPathLastUpdatedAt;
        IsSystem = isSystem;
    }

    public static AppCategory Create(
        DateTimeOffset createdAt,
        string name,
        string color,
        string? iconPath = null
    ) => new(Guid.CreateVersion7(), name, color, iconPath, createdAt, false);

    public static AppCategory Import(string name, string color, string? iconPath) =>
        new(Guid.CreateVersion7(), name, color, iconPath, DateTimeOffset.MinValue, false);

    public static AppCategory CreateUncategorized() =>
        new(UncategorizedId, "Uncategorized", "#8E8E93", null, DateTimeOffset.MinValue, true);

    public void Update(
        OptionalValue<string> name = default,
        OptionalValue<string> color = default,
        OptionalValue<string?> iconPath = default,
        DateTimeOffset? iconPathLastUpdatedAt = null
    )
    {
        if (iconPath.HasValue && iconPathLastUpdatedAt is null)
            throw new ArgumentException(
                "Icon path must be updated with a valid icon path last updated time."
            );

        if (name.HasValue && name.Value != Name)
            Name = name.Value;
        if (color.HasValue && color.Value != Color)
            Color = color.Value;
        if (iconPath.HasValue && iconPath.Value != IconPath)
        {
            IconPath = iconPath.Value;
            IconPathLastUpdatedAt = iconPathLastUpdatedAt!.Value;
        }
    }
}
