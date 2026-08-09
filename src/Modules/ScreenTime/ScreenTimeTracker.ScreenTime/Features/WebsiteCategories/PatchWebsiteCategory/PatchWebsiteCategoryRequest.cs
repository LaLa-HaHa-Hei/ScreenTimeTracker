using FastEndpoints;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.PatchWebsiteCategory;

public record PatchWebsiteCategoryRequest(
    [property: RouteParam] Guid WebsiteCategoryId,
    OptionalValue<string> Name,
    OptionalValue<string> Color,
    OptionalValue<string?> IconPath
);
