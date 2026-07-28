using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public record GetAppRequest([property: RouteParam] Guid AppId);
