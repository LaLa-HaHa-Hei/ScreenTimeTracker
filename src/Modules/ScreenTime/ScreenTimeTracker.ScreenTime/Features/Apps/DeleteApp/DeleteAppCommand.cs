using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.DeleteApp;

public record DeleteAppCommand(Guid AppId) : IRequest;
