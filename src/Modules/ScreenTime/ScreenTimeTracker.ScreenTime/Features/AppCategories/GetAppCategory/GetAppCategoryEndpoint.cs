using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public class GetAppCategoryEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoryRequest, GetAppCategoryResponse>
{
    public override void Configure()
    {
        Get("app-categories/{appCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppCategoryRequest req, CancellationToken ct)
    {
        var appCategory = await mediator.Send(new GetAppCategoryQuery(req.AppCategoryId), ct);

        if (appCategory is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(appCategory, ct);
    }
}
