namespace ScreenTimeTracker.ApplicationLayer.Common.DTOs;

public record AggregationSettingsDto(
    TimeSpan PollingInterval
);
