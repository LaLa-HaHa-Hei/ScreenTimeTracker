using ErrorOr;
using FastEndpoints;
using Mediator;
using Microsoft.AspNetCore.Http;

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

        ErrorOr<ImportDataResponse> result = await mediator.Send(
            new ImportDataCommand(rawJson),
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
