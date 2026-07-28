using FastEndpoints;
using Mediator;
using ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.CreateAppCategory;

public class CreateAppCategoryEndpoint(IMediator mediator)
    : Endpoint<CreateAppCategoryRequest, CreateAppCategoryResponse>
{
    public override void Configure()
    {
        Post("app-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAppCategoryRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(
            new CreateAppCategoryCommand(req.Name, req.Color, req.IconPath),
            ct
        );
        await Send.CreatedAtAsync<GetAppCategoryEndpoint>(
            new { id = result.Id },
            result,
            cancellation: ct
        );
    }
}
