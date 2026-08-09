using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.PatchUserSettings;

public class PatchUserSettingsEndpoint(IMediator mediator)
    : Endpoint<PatchUserSettingsRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("user-settings");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchUserSettingsRequest req, CancellationToken ct)
    {
        ErrorOr<Updated> result = await mediator.Send(
            new PatchUserSettingsCommand(
                req.AppTracking,
                req.WebsiteTracking,
                req.IdleDetection,
                req.TimeBoundary,
                req.Regional
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
