using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.DeleteUsageData;

public record DeleteUsageDataRequest(
    [property: QueryParam] DateOnly StartDate,
    [property: QueryParam] DateOnly EndDate
);
