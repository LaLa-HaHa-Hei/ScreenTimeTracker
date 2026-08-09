using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.DeleteWebsite;

public class DeleteWebsiteEndpoint(IMediator mediator)
    : Endpoint<DeleteWebsiteRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("websites/{websiteId}");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteWebsiteRequest req, CancellationToken ct)
    {
        ErrorOr<Deleted> result = await mediator.Send(
            new DeleteWebsiteCommand(WebsiteId: req.WebsiteId),
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
