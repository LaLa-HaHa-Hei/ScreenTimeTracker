using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetAllProcessInfos;

#pragma warning disable MSG0005 // MediatorGenerator message warning
public record GetAllProcessInfosQuery : IQuery<IEnumerable<ProcessInfoDto>>
#pragma warning restore MSG0005 // MediatorGenerator message warning
{
}