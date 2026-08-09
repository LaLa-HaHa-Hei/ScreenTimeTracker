using ErrorOr;
using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public record PatchAppCommand(
    Guid AppId,
    OptionalValue<string> Name = default,
    OptionalValue<string> Color = default,
    OptionalValue<bool> AllowMetadataAutoRefresh = default,
    OptionalValue<Guid> CategoryId = default,
    OptionalValue<string?> IconPath = default
) : IRequest<ErrorOr<Updated>>;
