namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategories;

public record GetAppCategoriesResponseItem(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
