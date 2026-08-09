using ErrorOr;
using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public record PatchAppCategoryCommand(
    Guid AppCategoryId,
    OptionalValue<string> Name = default,
    OptionalValue<string> Color = default,
    OptionalValue<string?> IconPath = default
) : IRequest<ErrorOr<Updated>>;
