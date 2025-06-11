using System.Diagnostics;

namespace Tracker.Services.Interfaces
{
    internal interface IForegroundWindowService
    {
        public Process? GetForegroundWindowProcess();
    }
}
