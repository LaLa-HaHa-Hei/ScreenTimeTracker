using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.DeleteWebsite;

public record DeleteWebsiteCommand(Guid WebsiteId) : IRequest<ErrorOr<Deleted>>;
