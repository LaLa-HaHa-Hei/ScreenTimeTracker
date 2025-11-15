using System.Diagnostics;

namespace ScreenTimeTracker.Application.Interfaces
{
    public interface IForegroundWindowService
    {
        Task<Process?> GetForegroundProcessAsync();
    }
}
