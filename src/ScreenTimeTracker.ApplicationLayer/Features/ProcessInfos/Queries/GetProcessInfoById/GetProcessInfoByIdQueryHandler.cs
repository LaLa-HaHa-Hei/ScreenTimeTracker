using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessInfoById;

public class GetProcessInfoByIdQueryHandler(IProcessInfosReadService readService) : IQueryHandler<GetProcessInfoByIdQuery, ProcessInfoDto?>
{
    private readonly IProcessInfosReadService _readService = readService;

    public async ValueTask<ProcessInfoDto?> Handle(GetProcessInfoByIdQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetProcessInfoById(query.Id, cancellationToken);
    }
}
