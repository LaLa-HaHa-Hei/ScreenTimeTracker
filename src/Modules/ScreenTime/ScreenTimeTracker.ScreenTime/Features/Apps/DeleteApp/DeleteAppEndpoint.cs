using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

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
        ErrorOr<Deleted> result = await mediator.Send(new DeleteAppCommand(AppId: req.AppId), ct);

        if (result.IsError)
        {
            var firstError = result.FirstError;
            await Send.ResultAsync(
                Results.Problem(
                    detail: firstError.Description,
                    statusCode: firstError.Type switch
                    {
                        ErrorType.NotFound => StatusCodes.Status404NotFound,
                        ErrorType.Conflict => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status500InternalServerError,
                    },
                    extensions: new Dictionary<string, object?> { ["code"] = firstError.Code }
                )
            );
            return;
        }

        await Send.NoContentAsync(ct);
    }
}
