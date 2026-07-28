using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApps;

public class GetAppsEndpoint(IMediator mediator)
    : Endpoint<GetAppsRequest, List<Dictionary<string, object?>>>
{
    public override void Configure()
    {
        Get("apps");
        Group<ScreenTimeGroup>();
        AllowAnonymous();

        Description(d => d.Produces<List<GetAppsResponseItem>>(200, "application/json"));
    }

    public override async Task HandleAsync(GetAppsRequest req, CancellationToken ct)
    {
        var apps = await mediator.Send(new GetAppsQuery(req.Fields), ct);
        await Send.OkAsync(apps, ct);
    }
}
