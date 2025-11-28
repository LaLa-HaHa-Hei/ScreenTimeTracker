using Mediator;
using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.DomainLayer.Entities;
using ScreenTimeTracker.DomainLayer.Interfaces;

namespace ScreenTimeTracker.ApplicationLayer.Features.Configuration.Queries.GetAggregationSettings;

public class GetAggregationSettingsQueryHandler(IUserConfigurationRepository userConfigRepository) : IQueryHandler<GetAggregationSettingsQuery, AggregationSettingsDto>
{
    private readonly IUserConfigurationRepository _userConfigRepository = userConfigRepository;

    public async ValueTask<AggregationSettingsDto> Handle(GetAggregationSettingsQuery query, CancellationToken cancellationToken)
    {
        UserConfiguration userConfig = await _userConfigRepository.GetConfig();
        AggregationSettings setting = userConfig.Aggregation;
        return new AggregationSettingsDto(
            PollingInterval: setting.PollingInterval
        );
    }
}