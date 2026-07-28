using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public class PatchAppCategoryEndpoint(IMediator mediator)
    : Endpoint<PatchAppCategoryRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("app-categories/{appCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchAppCategoryRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new PatchAppCategoryCommand(
                AppCategoryId: req.AppCategoryId,
                Name: req.Name,
                Color: req.Color,
                IconPath: req.IconPath
            ),
            ct
        );
        await Send.NoContentAsync(ct);
    }
}
