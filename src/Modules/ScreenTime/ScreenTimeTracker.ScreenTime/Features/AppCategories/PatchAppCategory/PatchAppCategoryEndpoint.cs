using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

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
        ErrorOr<Updated> result = await mediator.Send(
            new PatchAppCategoryCommand(
                AppCategoryId: req.AppCategoryId,
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
