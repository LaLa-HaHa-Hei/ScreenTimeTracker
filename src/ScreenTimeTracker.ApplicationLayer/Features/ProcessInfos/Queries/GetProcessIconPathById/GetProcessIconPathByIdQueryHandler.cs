using Mediator;
using ScreenTimeTracker.ApplicationLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.ProcessInfos.Queries.GetProcessIconPathById;

public class GetProcessIconPathByIdQueryHandler(IProcessInfosReadService readService) : IQueryHandler<GetProcessIconPathByIdQuery, string?>
{
    private readonly IProcessInfosReadService _readService = readService;

    public async ValueTask<string?> Handle(GetProcessIconPathByIdQuery query, CancellationToken cancellationToken)
    {
        return await _readService.GetProcessIconPathById(query.Id, cancellationToken);
    }
}
