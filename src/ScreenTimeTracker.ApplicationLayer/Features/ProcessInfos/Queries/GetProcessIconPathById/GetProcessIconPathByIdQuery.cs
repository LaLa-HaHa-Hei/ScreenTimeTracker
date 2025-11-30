using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessIconPathById;

public record GetProcessIconPathByIdQuery(Guid Id) : IQuery<string?>
{
}