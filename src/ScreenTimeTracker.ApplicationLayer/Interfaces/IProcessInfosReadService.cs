using ScreenTimeTracker.ApplicationLayer.Common.DTOs;

namespace ScreenTimeTracker.ApplicationLayer.Interfaces;

public interface IProcessInfosReadService
{
    Task<IEnumerable<ProcessInfoDto>> GetAllProcessInfos(CancellationToken cancellationToken = default);
    Task<string?> GetProcessIconPathById(Guid id, CancellationToken cancellationToken = default);
    Task<ProcessInfoDto?> GetProcessInfoById(Guid id, CancellationToken cancellationToken = default);
}