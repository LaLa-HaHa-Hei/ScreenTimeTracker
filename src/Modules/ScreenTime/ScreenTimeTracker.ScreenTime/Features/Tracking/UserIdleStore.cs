namespace ScreenTimeTracker.ScreenTime.Features.Tracking;

public record UserIdleState(bool IsUserIdle);

public class UserIdleStore
{
    public UserIdleState Current { get; set; } = new(false);
}
