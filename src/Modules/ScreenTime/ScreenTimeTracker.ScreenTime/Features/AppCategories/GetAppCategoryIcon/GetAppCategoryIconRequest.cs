using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategoryIcon;

public record GetAppCategoryIconRequest([property: RouteParam] Guid AppCategoryId);
