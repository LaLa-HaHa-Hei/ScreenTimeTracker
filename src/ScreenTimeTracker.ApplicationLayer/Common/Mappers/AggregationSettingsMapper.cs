using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.ApplicationLayer.Common.Mappers;

internal class AggregationSettingsMapper
{
    public static AggregationSettings Map(AggregationSettingsDto entity)
    {
        return new AggregationSettings(
            entity.PollingInterval
        );
    }

    public static AggregationSettingsDto Map(AggregationSettings domain)
    {
        return new AggregationSettingsDto
        (
            PollingInterval: domain.PollingInterval
        );
    }
}

