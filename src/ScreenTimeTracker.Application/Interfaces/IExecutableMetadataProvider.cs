using ScreenTimeTracker.Application.DTOs;

namespace ScreenTimeTracker.Application.Interfaces
{
    public interface IExecutableMetadataProvider
    {
        public Task<ExecutableMetadata> GetMetadataAsync(string executablePath);
    }
}
