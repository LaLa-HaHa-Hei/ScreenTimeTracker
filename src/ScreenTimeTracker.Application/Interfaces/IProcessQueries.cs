using ScreenTimeTracker.Application.DTOs;

namespace ScreenTimeTracker.Application.Interfaces
{
    public interface IProcessQueries
    {
        Task<IEnumerable<ProcessInfoDto>> GetAllProcessesAsync(CancellationToken cancellationToken = default);
        Task<string?> GetProcessIconPathByIdAsync(Guid processId, CancellationToken cancellationToken = default);
        Task<ProcessInfoDto?> GetProcessByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}