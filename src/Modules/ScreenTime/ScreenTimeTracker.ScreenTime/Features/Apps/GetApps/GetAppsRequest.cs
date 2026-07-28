using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApps;

public record GetAppsRequest([property: QueryParam] string? Fields);
