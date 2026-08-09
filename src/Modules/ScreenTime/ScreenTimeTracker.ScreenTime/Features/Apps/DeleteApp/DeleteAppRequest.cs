using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.DeleteApp;

public record DeleteAppRequest([property: RouteParam] Guid AppId);
