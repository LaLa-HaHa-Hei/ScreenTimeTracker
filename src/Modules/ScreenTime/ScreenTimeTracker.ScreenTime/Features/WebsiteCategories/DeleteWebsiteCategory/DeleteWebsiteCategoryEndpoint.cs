using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.DeleteWebsiteCategory;

public class DeleteWebsiteCategoryEndpoint(IMediator mediator)
    : Endpoint<DeleteWebsiteCategoryRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("website-categories/{websiteCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteWebsiteCategoryRequest req, CancellationToken ct)
    {
        ErrorOr<Deleted> result = await mediator.Send(
            new DeleteWebsiteCategoryCommand(WebsiteCategoryId: req.WebsiteCategoryId),
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
