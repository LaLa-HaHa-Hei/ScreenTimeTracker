using FastEndpoints;
using Mediator;
using ScreenTimeTracker.DesktopSettings.Contracts.Queries;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.GetLocalSettings;

public class GetLocalSettingsEndpoint(IMediator mediator)
    : Endpoint<EmptyRequest, GetLocalSettingsResult>
{
    public override void Configure()
    {
        Get("local-settings");
        Group<DesktopSettingsGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        GetLocalSettingsResult response = await mediator.Send(new GetLocalSettingsQuery(), ct);
        await Send.OkAsync(response, ct);
    }
}
