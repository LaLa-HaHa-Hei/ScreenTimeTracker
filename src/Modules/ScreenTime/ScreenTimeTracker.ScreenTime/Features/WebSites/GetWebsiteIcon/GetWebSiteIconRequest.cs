using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteIcon;

public record GetWebsiteIconRequest([property: RouteParam] Guid WebsiteId);
