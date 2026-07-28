using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public class PatchLocalSettingsEndpoint(IMediator mediator)
    : Endpoint<PatchLocalSettingsRequest, EmptyResponse>
{
    public override void Configure()
    {
        Patch("local-settings");
        Group<DesktopSettingsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(PatchLocalSettingsRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new PatchLocalSettingsCommand(
                req.DefaultUIOpenMode,
                req.IsAutoStartEnabled,
                req.IsSilentStartEnabled,
                req.Language
            ),
            ct
        );
        await Send.NoContentAsync(ct);
    }
}
