
using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.DomainLayer.Interfaces
{
    public interface IActivityIntervalRepository
    {
        Task AddAsync(ActivityInterval interval);
        Task UpdateRangeAsync(IEnumerable<ActivityInterval> intervals);
        Task RemoveRangeAsync(IEnumerable<ActivityInterval> intervals);
        Task<IEnumerable<ActivityInterval>> GetByTimestampBeforeAsync(DateTime timestamp);
        Task<IEnumerable<ActivityInterval>> GetNonIdleByTimestampAfterAsync(DateTime timestamp);
    }
}
