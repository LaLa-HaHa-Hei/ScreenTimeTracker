using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsites;

public class GetWebsitesEndpoint(IMediator mediator)
    : Endpoint<GetWebsitesRequest, List<Dictionary<string, object?>>>
{
    public override void Configure()
    {
        Get("websites");
        Group<ScreenTimeGroup>();
        AllowAnonymous();

        Description(d => d.Produces<List<GetWebsitesResponseItem>>(200, "websitelication/json"));
    }

    public override async Task HandleAsync(GetWebsitesRequest req, CancellationToken ct)
    {
        var websites = await mediator.Send(new GetWebsitesQuery(req.Fields), ct);
        await Send.OkAsync(websites, ct);
    }
}
