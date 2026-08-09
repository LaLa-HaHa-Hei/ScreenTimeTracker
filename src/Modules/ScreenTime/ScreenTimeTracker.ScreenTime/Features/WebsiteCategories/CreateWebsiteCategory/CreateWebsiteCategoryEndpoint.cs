using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;
using ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.CreateWebsiteCategory;

public class CreateWebsiteCategoryEndpoint(IMediator mediator)
    : Endpoint<CreateWebsiteCategoryRequest, CreateWebsiteCategoryResponse>
{
    public override void Configure()
    {
        Post("website-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateWebsiteCategoryRequest req, CancellationToken ct)
    {
        ErrorOr<CreateWebsiteCategoryResponse> result = await mediator.Send(
            new CreateWebsiteCategoryCommand(req.Name, req.Color, req.IconPath),
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
                        ErrorType.Conflict => StatusCodes.Status409Conflict,
                        _ => StatusCodes.Status500InternalServerError,
                    },
                    extensions: new Dictionary<string, object?> { ["code"] = firstError.Code }
                )
            );
            return;
        }

        await Send.CreatedAtAsync<GetWebsiteCategoryEndpoint>(
            new { id = result.Value.Id },
            result.Value,
            cancellation: ct
        );
    }
}
