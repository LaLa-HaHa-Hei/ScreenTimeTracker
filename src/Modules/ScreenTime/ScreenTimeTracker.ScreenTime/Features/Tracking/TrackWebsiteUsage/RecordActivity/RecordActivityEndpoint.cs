using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage.RecordActivity;

public class RecordActivityEndpoint(
    WebsiteActivityProcessor websiteActivityProcessor,
    TimeProvider timeProvider
) : Endpoint<RecordActivityRequest>
{
    public override void Configure()
    {
        Post("tracking/track-website-usage/record-activity");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(RecordActivityRequest req, CancellationToken ct)
    {
        websiteActivityProcessor.Enqueue(
            new(
                req.Host,
                req.Name,
                TimeSpan.FromMilliseconds(req.DurationMilliseconds),
                timeProvider.GetUtcNow()
            )
        );
        await Send.NoContentAsync(ct);
    }
}
