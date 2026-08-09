using FastEndpoints;
using Mediator;
using ScreenTimeTracker.DesktopSettings.Contracts.Queries;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.GetLocalSettings;

public class GetLocalSettingsEndpoint(IMediator mediator)
    : Endpoint<EmptyRequest, GetLocalSettingsResponse>
{
    public override void Configure()
    {
        Get("local-settings");
        Group<DesktopSettingsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var response = await mediator.Send(new GetLocalSettingsQuery(), ct);
        await Send.OkAsync(
            new GetLocalSettingsResponse(
                response.DefaultUIOpenMode,
                response.IsAutoStartEnabled,
                response.IsSilentStartEnabled,
                response.Language
            ),
            ct
        );
    }
}
