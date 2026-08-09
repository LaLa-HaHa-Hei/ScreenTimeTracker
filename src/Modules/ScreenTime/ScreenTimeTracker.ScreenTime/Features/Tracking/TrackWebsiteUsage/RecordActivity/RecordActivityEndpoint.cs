using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage.RecordActivity;

public class RecordActivityEndpoint(IMediator mediator) : Endpoint<RecordActivityRequest>
{
    public override void Configure()
    {
        Post("tracking/track-website-usage/record-activity");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(RecordActivityRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new RecordActivityCommand(
                req.Host,
                req.Name,
                TimeSpan.FromMilliseconds(req.DurationMilliseconds)
            ),
            ct
        );
        await Send.OkAsync(result, ct);
    }
}
