using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public class GetAppCategoryEndpoint(IMediator mediator)
    : Endpoint<GetAppCategoryRequest, GetAppCategoryResponse>
{
    public override void Configure()
    {
        Get("app-categories/{appCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppCategoryRequest req, CancellationToken ct)
    {
        ErrorOr<GetAppCategoryResponse> result = await mediator.Send(
            new GetAppCategoryQuery(req.AppCategoryId),
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
                        _ => StatusCodes.Status500InternalServerError,
                    },
                    extensions: new Dictionary<string, object?> { ["code"] = firstError.Code }
                )
            );
            return;
        }

        await Send.OkAsync(result.Value, ct);
    }
}
