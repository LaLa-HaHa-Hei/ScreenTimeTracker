using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ExportData;

public class ExportDataEndpoint(IMediator mediator) : EndpointWithoutRequest<ExportDataResponse>
{
    public override void Configure()
    {
        Get("data/export");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = await mediator.Send(new ExportDataQuery(), ct);
        await Send.OkAsync(response, ct);
    }
}
