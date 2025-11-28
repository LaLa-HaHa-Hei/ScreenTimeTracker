using Mediator;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessIconPathById;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetProcessIconPathByIdQuery(Guid Id) : IQuery<string?>
#pragma warning restore MSG0005 // MediatorGenerator message warning
{
}