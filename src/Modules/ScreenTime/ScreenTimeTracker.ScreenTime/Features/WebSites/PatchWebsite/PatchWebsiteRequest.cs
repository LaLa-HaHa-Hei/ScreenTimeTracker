using FastEndpoints;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.PatchWebsite;

public record PatchWebsiteRequest(
    [property: RouteParam] Guid Id,
    OptionalValue<string> Name = default,
    OptionalValue<string> Color = default,
    OptionalValue<bool> AllowMetadataAutoRefresh = default,
    OptionalValue<Guid> CategoryId = default,
    OptionalValue<string?> IconPath = default
);
