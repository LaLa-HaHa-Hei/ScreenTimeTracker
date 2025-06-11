using System.Diagnostics;

namespace Tracker.Services.Interfaces
{
    internal interface IProcessInfoService
    {
        Task EnsureProcessInfoAsync(Process process);
    }
}
