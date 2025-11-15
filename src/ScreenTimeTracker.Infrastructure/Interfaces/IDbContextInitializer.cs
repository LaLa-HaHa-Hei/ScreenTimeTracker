namespace ScreenTimeTracker.Infrastructure.Interfaces
{
    public interface IDbContextInitializer
    {
        Task InitializeAsync();
    }
}