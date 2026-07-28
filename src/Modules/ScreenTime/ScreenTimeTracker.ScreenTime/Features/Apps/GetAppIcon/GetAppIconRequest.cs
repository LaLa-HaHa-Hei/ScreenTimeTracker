using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetAppIcon;

public record GetAppIconRequest([property: RouteParam] Guid AppId);
