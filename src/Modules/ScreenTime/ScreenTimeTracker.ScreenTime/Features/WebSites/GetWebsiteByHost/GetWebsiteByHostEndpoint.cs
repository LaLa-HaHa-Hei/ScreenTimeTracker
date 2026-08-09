using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsiteByHost;

public class GetWebsiteByHostEndpoint(IMediator mediator)
    : Endpoint<GetWebsiteByHostRequest, GetWebsiteByHostResponse>
{
    public override void Configure()
    {
        Get("websites/by-host/{host}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetWebsiteByHostRequest req, CancellationToken ct)
    {
        ErrorOr<GetWebsiteByHostResponse> result = await mediator.Send(
            new GetWebsiteByHostQuery(req.Host),
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
