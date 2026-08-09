namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.CreateWebsiteCategory;

public record CreateWebsiteCategoryResponse(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
