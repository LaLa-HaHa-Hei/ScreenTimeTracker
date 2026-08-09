using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

public class GetWebsiteCategoryEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteCategoryRequest, GetWebsiteCategoryResponse>
{
    public override void Configure()
    {
        Get("website-categories/{websiteCategoryId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWebsiteCategoryRequest req, CancellationToken ct)
    {
        ErrorOr<GetWebsiteCategoryResponse> result = await mediator.Send(
            new GetWebsiteCategoryQuery(req.WebsiteCategoryId),
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
