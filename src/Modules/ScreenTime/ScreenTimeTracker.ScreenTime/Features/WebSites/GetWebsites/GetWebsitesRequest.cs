using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsites;

public record GetWebsitesRequest([property: QueryParam] string? Fields);
