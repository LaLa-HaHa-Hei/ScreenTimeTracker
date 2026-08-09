namespace ScreenTimeTracker.ScreenTime.Features.Tracking.TrackWebsiteUsage.RecordActivity;

public record RecordActivityRequest(string Host, string Name, long DurationMilliseconds);
