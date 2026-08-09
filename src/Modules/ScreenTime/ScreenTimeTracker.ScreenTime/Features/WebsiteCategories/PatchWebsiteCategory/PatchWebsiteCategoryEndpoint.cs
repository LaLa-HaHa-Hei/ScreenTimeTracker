using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.PatchWebsiteCategory;

public class PatchWebsiteCategoryEndpoint(IMediator mediator)
    : Endpoint<PatchWebsiteCategoryRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("website-categories/{websiteCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchWebsiteCategoryRequest req, CancellationToken ct)
    {
        ErrorOr<Updated> result = await mediator.Send(
            new PatchWebsiteCategoryCommand(
                WebsiteCategoryId: req.WebsiteCategoryId,
                Name: req.Name,
                Color: req.Color,
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
