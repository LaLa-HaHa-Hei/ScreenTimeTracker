using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.Desktop.WebApi.DTOs;

namespace ScreenTimeTracker.Desktop.WebApi.Mappers;

internal class AggregationSettingsMapper
{
    public static AggregationSettingsResponse Map(AggregationSettingsDto source)
    {
        return new AggregationSettingsResponse((int)source.PollingInterval.TotalMinutes);
    }

    public static AggregationSettingsDto Map(UpdateAggregationSettingsRequest source)
    {
        return new AggregationSettingsDto(TimeSpan.FromMinutes(source.PollingIntervalMinutes));
    }
}
