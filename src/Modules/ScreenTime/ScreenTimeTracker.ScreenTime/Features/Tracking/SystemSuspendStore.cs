namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public record SystemSuspendState(bool IsSystemSuspendActive);

public class SystemSuspendStore
{
    public SystemSuspendState Current { get; set; } = new(false);
}
