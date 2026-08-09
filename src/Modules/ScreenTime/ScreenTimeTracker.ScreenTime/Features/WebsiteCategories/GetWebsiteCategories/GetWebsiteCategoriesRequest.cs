using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategories;

public record GetWebsiteCategoriesRequest([property: QueryParam] string? Fields);
