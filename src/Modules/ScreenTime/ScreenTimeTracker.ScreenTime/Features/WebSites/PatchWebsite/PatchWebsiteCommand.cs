using ErrorOr;
using Mediator;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.PatchWebsite;

public record PatchWebsiteCommand(
    Guid WebsiteId,
    OptionalValue<string> Name = default,
    OptionalValue<string> Color = default,
    OptionalValue<bool> AllowMetadataAutoRefresh = default,
    OptionalValue<Guid> CategoryId = default,
    OptionalValue<string?> IconPath = default
) : IRequest<ErrorOr<Updated>>;
