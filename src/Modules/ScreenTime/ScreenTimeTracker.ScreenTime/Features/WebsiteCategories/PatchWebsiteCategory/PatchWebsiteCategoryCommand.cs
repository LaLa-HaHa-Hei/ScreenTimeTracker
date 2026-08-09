using ErrorOr;
using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.PatchWebsiteCategory;

public record PatchWebsiteCategoryCommand(
    Guid WebsiteCategoryId,
    OptionalValue<string> Name,
    OptionalValue<string> Color,
    OptionalValue<string?> IconPath
) : IRequest<ErrorOr<Updated>>;
