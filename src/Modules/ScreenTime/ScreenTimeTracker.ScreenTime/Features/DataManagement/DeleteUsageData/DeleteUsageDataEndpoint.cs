using FastEndpoints;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.DeleteUsageData;

public class DeleteUsageDataEndpoint(IMediator mediator)
    : Endpoint<DeleteUsageDataRequest, EmptyResponse>
{
    public override void Configure()
    {
        Delete("data");
        Group<ScreenTimeGroup>();
        AllowAnonymous();
    }

    public override async Task HandleAsync(DeleteUsageDataRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new DeleteUsageDataCommand(
                StartDate: req.StartDate,
                EndDate: req.EndDate,
                TimeZoneInfo: TimeZoneInfo.FindSystemTimeZoneById(req.TimeZoneId)
            ),
            ct
        );
        await Send.NoContentAsync(ct);
    }
}
