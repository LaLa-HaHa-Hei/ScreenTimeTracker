using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;
using ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.CreateAppCategory;

public class CreateAppCategoryEndpoint(IMediator mediator)
    : Endpoint<CreateAppCategoryRequest, CreateAppCategoryResponse>
{
    public override void Configure()
    {
        Post("app-categories");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAppCategoryRequest req, CancellationToken ct)
    {
        ErrorOr<CreateAppCategoryResponse> result = await mediator.Send(
            new CreateAppCategoryCommand(req.Name, req.Color, req.IconPath),
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

        await Send.CreatedAtAsync<GetAppCategoryEndpoint>(
            new { id = result.Value.Id },
            result.Value,
            cancellation: ct
        );
    }
}
