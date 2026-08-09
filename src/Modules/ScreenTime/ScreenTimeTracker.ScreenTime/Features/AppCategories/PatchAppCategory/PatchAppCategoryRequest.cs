using FastEndpoints;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public record PatchAppCategoryRequest(
    [property: RouteParam] Guid AppCategoryId,
    OptionalValue<string> Name = default,
    OptionalValue<string> Color = default,
    OptionalValue<string?> IconPath = default
);
