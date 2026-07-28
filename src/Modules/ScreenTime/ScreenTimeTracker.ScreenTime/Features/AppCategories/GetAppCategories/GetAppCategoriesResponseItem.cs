namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategories;

public record GetAppCategoriesResponseItem(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTime IconPathLastUpdatedAt,
    bool IsSystem
);
