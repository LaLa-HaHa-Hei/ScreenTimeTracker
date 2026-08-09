using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public class PatchAppEndpoint(IMediator mediator) : Endpoint<PatchAppRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("apps/{appId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchAppRequest req, CancellationToken ct)
    {
        ErrorOr<Updated> result = await mediator.Send(
            new PatchAppCommand(
                AppId: req.AppId,
                Name: req.Name,
                Color: req.Color,
                AllowMetadataAutoRefresh: req.AllowMetadataAutoRefresh,
                CategoryId: req.CategoryId,
                IconPath: req.IconPath
            ),
            ct
        );

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
