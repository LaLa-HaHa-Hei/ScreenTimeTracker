using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.DeleteUsageData;

public record DeleteUsageDataCommand(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeSpan MinDuration = default
) : IRequest;
