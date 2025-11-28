using System.Diagnostics;

namespace ScreenTimeTracker.ApplicationLayer.Common.Interfaces;

public interface IForegroundWindowService
{
    Task<Process?> GetForegroundProcessAsync();
}
