using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

public record GetWebsiteCategoryRequest([property: RouteParam] Guid WebsiteCategoryId);
