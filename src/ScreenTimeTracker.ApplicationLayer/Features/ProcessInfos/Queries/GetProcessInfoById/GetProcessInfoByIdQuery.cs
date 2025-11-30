using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessInfoById;

public record GetProcessInfoByIdQuery(Guid Id) : IQuery<ProcessInfoDto>
{
}