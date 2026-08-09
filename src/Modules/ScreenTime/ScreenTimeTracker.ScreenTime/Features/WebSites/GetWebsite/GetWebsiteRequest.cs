using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsite;

public record GetWebsiteRequest([property: RouteParam] Guid WebsiteId);
