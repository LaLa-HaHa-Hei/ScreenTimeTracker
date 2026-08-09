using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.DeleteAppCategory;

public record DeleteAppCategoryRequest([property: RouteParam] Guid AppCategoryId);
