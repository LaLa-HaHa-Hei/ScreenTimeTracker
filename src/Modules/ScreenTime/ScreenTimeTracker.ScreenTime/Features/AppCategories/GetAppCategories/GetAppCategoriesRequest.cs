using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategories;

public record GetAppCategoriesRequest([property: QueryParam] string? Fields);
