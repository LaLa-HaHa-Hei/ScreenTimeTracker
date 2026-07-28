using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public record GetAppCategoryRequest([property: RouteParam] Guid AppCategoryId);
