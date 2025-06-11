namespace Tracker.Services.Interfaces
{
    internal interface IUsageAggregator
    {
        void AddUsage(string processName, int durationMs);
        Task SaveAsync();
    }
}
