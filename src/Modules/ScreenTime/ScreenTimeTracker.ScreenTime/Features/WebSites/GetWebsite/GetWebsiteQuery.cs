using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsite;

public record GetWebsiteQuery(Guid WebsiteId) : IRequest<ErrorOr<GetWebsiteResponse>>;
