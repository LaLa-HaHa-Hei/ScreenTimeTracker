using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategories;

public class GetAppCategoriesEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoriesRequest, List<Dictionary<string, object?>>>
{
    public override void Configure()
    {
        Get("app-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();

        Description(d => d.Produces<List<GetAppCategoriesResponseItem>>(200, "application/json"));
    }

    public override async Task HandleAsync(GetAppCategoriesRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAppCategoriesQuery(req.Fields), ct);
        await Send.OkAsync(result, ct);
    }
}
