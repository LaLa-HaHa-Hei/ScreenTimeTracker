namespace ScreenTimeTracker.Application.Interfaces
{
    public interface IIdleTimeProvider
    {
        Task<TimeSpan> GetSystemIdleTimeAsync();
    }
}
