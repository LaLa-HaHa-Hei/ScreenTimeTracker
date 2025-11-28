using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.DomainLayer.Interfaces;

public interface IProcessInfoRepository
{
    Task AddAsync(ProcessInfo processInfo);
    Task<ProcessInfo?> GetByIdAsync(Guid id);
    Task<ProcessInfo?> GetByNameAsync(string name);
    Task UpdateAsync(ProcessInfo processInfo);
    Task DeleteAsync(Guid id);
}
