using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public record UserActiveCommand : IRequest;

public class UserActiveHandler(IForegroundWindowMonitor foregroundWindowMonitor, IMediator mediator)
    : IRequestHandler<UserActiveCommand>
{
    public async ValueTask<Unit> Handle(
        UserActiveCommand request,
        CancellationToken cancellationToken
    )
    {
        var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
        await mediator.Send(new ForegroundWindowChangedCommand(windowInfo), cancellationToken);

        return Unit.Value;
    }
}
