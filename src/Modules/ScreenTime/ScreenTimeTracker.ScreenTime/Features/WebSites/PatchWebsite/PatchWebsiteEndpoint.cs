using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.PatchWebsite;

public class PatchWebsiteEndpoint(IMediator mediator) : Endpoint<PatchWebsiteRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("websites/{Id}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchWebsiteRequest req, CancellationToken ct)
    {
        ErrorOr<Updated> result = await mediator.Send(
            new PatchWebsiteCommand(
                req.Id,
                req.Name,
                req.Color,
                req.AllowMetadataAutoRefresh,
                req.CategoryId,
                req.IconPath
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
