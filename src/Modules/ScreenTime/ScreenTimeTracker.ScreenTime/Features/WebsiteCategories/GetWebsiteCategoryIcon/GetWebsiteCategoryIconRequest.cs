using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategoryIcon;

public record GetWebsiteCategoryIconRequest([property: RouteParam] Guid WebsiteCategoryId);
