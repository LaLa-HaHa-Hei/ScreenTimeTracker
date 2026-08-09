using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public class GetAppEndpoint(IMediator mediator) : Endpoint<GetAppRequest, GetAppResponse>
{
    public override void Configure()
    {
        Get("apps/{appId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetAppRequest req, CancellationToken ct)
    {
        ErrorOr<GetAppResponse> result = await mediator.Send(new GetAppQuery(req.AppId), ct);

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
