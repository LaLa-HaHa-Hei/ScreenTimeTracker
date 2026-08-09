using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.RefreshWebsiteMetadata;

public class RefreshWebsiteMetadataEndpoint(IMediator mediator)
    : Endpoint<RefreshWebsiteMetadataRequest, EmptyResponse>
{
    public override void Configure()
    {
        Post("websites/{Id}/refresh-metadata");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(RefreshWebsiteMetadataRequest req, CancellationToken ct)
    {
        ErrorOr<Updated> result = await mediator.Send(
            new RefreshWebsiteMetadataCommand(Id: req.Id, Icon: req.Icon),
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
