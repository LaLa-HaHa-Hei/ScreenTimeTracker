using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

public record SystemResumedCommand : IRequest;

public class SystemResumedHandlerHandler(
    IForegroundWindowMonitor foregroundWindowMonitor,
    IMediator mediator
) : IRequestHandler<SystemResumedCommand>
{
    public async ValueTask<Unit> Handle(
        SystemResumedCommand request,
        CancellationToken cancellationToken
    )
    {
        var windowInfo = foregroundWindowMonitor.GetForegroundWindow();
        await mediator.Send(new ForegroundWindowChangedCommand(windowInfo), cancellationToken);

        return Unit.Value;
    }
}
