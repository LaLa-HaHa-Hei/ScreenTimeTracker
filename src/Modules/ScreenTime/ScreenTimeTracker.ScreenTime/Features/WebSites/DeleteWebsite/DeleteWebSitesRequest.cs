using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.DeleteWebsite;

public record DeleteWebsiteRequest([property: RouteParam] Guid WebsiteId);
