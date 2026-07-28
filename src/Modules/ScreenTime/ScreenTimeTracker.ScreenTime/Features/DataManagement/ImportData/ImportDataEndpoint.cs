using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ImportData;

public class ImportDataEndpoint(IMediator mediator) : EndpointWithoutRequest<ImportDataResponse>
{
    public override void Configure()
    {
        Post("data/import");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var rawJson = await reader.ReadToEndAsync(ct);

        try
        {
            var response = await mediator.Send(new ImportDataCommand(rawJson), ct);
            await Send.OkAsync(response, ct);
        }
        catch (NotSupportedException)
        {
            await Send.ErrorsAsync(statusCode: 422, ct);
        }
    }
}
