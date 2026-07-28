namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public record GetAppCategoryResponse(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTime IconPathLastUpdatedAt,
    bool IsSystem
);
