using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage.RecordActivity;

public record RecordActivityCommand(string Host, string Name, TimeSpan Duration) : IRequest;
