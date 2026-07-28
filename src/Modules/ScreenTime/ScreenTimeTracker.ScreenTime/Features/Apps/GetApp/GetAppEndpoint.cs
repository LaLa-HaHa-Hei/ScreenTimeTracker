using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public class GetAppEndpoint(IMediator mediator) : Endpoint<GetAppRequest, GetAppResponse>
{
    public override void Configure()
    {
        Get("apps/{appId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppRequest req, CancellationToken ct)
    {
        var app = await mediator.Send(new GetAppQuery(req.AppId), ct);

        if (app is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(app, ct);
    }
}
