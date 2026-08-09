namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

public record GetWebsiteCategoryResponse(
    Guid Id,
    string Name,
    string Color,
    string? IconPath,
    DateTimeOffset IconPathLastUpdatedAt,
    bool IsSystem
);
