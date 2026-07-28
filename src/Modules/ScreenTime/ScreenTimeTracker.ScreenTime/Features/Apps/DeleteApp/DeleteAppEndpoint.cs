using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.DeleteApp;

public class DeleteAppEndpoint(IMediator mediator) : Endpoint<DeleteAppRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("apps/{appId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteAppRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteAppCommand(AppId: req.AppId), ct);
        await Send.NoContentAsync(ct);
    }
}
