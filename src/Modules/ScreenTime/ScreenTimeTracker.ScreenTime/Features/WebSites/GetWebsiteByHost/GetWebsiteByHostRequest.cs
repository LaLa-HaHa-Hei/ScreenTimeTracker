using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteByHost;

public record GetWebsiteByHostRequest([property: RouteParam] string Host);
