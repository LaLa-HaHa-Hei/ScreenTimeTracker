using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public record GetAppQuery(Guid AppId) : IRequest<GetAppResponse?>;
