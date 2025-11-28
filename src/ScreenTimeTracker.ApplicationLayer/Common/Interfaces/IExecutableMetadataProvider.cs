using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Common.Interfaces;

public interface IExecutableMetadataProvider
{
    public Task<ExecutableMetadata> GetMetadataAsync(string executablePath);
}

