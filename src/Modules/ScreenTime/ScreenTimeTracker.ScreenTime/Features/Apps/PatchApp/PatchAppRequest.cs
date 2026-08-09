using FastEndpoints;
using ScreenTimeTracker.BuildingBlocks.Types;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public record PatchAppRequest(
    [property: RouteParam] Guid AppId,
    OptionalValue<string> Name = default,
    OptionalValue<string> Color = default,
    OptionalValue<bool> AllowMetadataAutoRefresh = default,
    OptionalValue<Guid> CategoryId = default,
    OptionalValue<string?> IconPath = default
);
