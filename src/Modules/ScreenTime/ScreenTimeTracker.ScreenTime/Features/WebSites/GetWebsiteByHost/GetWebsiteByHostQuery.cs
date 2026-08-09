using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteByHost;

public record GetWebsiteByHostQuery(string Host) : IRequest<ErrorOr<GetWebsiteByHostResponse>>;
