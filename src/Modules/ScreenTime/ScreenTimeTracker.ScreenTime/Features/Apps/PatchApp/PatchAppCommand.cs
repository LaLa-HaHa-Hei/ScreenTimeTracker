using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public record PatchAppCommand(
    Guid Id,
    OptionalValue<string> Name,
    OptionalValue<string> Color,
    OptionalValue<bool> AllowMetadataAutoUpdate,
    OptionalValue<Guid> AppCategoryId,
    OptionalValue<string?> IconPath
) : IRequest;
