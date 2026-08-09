using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.DeleteWebsiteCategory;

public record DeleteWebsiteCategoryRequest([property: RouteParam] Guid WebsiteCategoryId);
