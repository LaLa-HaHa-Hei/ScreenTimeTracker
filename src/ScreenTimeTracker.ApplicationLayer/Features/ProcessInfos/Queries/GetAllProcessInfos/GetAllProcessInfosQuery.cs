using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetAllProcessInfos;

public record GetAllProcessInfosQuery : IQuery<IEnumerable<ProcessInfoDto>>
{
}