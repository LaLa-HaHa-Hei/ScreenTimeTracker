using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.RefreshWebsiteMetadata;

public record RefreshWebsiteMetadataRequest([property: RouteParam] Guid Id, Icon? Icon);
