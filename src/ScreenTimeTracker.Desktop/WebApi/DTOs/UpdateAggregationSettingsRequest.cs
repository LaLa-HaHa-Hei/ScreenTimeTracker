namespace ScreenTimeTracker.Desktop.WebApi.DTOs;

public record UpdateAggregationSettingsRequest(
    int PollingIntervalMinutes
);
