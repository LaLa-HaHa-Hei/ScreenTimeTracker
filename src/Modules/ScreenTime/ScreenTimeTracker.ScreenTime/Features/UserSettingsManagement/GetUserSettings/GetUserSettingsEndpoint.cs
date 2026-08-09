using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.UserSettingsManagement.GetUserSettings;

public class GetUserSettingsEndpoint(IMediator mediator)
    : Endpoint<EmptyRequest, GetUserSettingsResponse>
{
    public override void Configure()
    {
        Get("user-settings");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(EmptyRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUserSettingsQuery(), ct);
        await Send.OkAsync(result, ct);
    }
}
