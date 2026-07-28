using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public class PatchAppEndpoint(IMediator mediator) : Endpoint<PatchAppRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("apps/{Id}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchAppRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new PatchAppCommand(
                Id: req.Id,
                Name: req.Name,
                Color: req.Color,
                AllowMetadataAutoUpdate: req.AllowMetadataAutoUpdate,
                AppCategoryId: req.AppCategoryId,
                IconPath: req.IconPath
            ),
            ct
        );
        await Send.NoContentAsync(ct);
    }
}
