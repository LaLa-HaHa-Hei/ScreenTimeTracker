using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessInfoById;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetProcessInfoByIdQuery(Guid Id) : IQuery<ProcessInfoDto>
#pragma warning restore MSG0005 // MediatorGenerator message warning
{
}