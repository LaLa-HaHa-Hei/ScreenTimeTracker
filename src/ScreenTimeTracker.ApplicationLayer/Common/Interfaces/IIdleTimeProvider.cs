namespace ScreenTimeTracker.ApplicationLayer.Common.Interfaces;

public interface IIdleTimeProvider
{
    Task<TimeSpan> GetSystemIdleTimeAsync();
}

