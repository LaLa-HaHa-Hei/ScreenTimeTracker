using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public record PatchAppCategoryCommand(
    Guid AppCategoryId,
    OptionalValue<string> Name,
    OptionalValue<string> Color,
    OptionalValue<string?> IconPath
) : IRequest;
