using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategories;

public class GetWebsiteCategoriesEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteCategoriesRequest, List<Dictionary<string, object?>>>
{
    public override void Configure()
    {
        Get("website-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();

        Description(d =>
            d.Produces<List<GetWebsiteCategoriesResponseItem>>(200, "websitelication/json")
        );
    }

    public override async Task HandleAsync(GetWebsiteCategoriesRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetWebsiteCategoriesQuery(req.Fields), ct);
        await Send.OkAsync(result, ct);
    }
}
