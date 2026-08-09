namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.CreateAppCategory;

public record CreateAppCategoryResponse(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
