using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetAllProcessInfos;

public class GetAllProcessInfosQueryHandler(IProcessInfosReadService readService) : IQueryHandler<GetAllProcessInfosQuery, IEnumerable<ProcessInfoDto>>
{
    private readonly IProcessInfosReadService _readService = readService;

    public async ValueTask<IEnumerable<ProcessInfoDto>> Handle(GetAllProcessInfosQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetAllProcessInfos(cancellationToken);
    }
}
