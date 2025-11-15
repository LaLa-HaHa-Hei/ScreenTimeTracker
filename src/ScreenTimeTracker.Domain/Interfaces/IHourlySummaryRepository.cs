using ScreenTimeTracker.Domain.Entities;

namespace ScreenTimeTracker.Domain.Interfaces
{
    public interface IHourlySummaryRepository
    {
        Task AddAsync(HourlySummary hourlySummary);
        Task UpdateAsync(HourlySummary hourlySummary);
        Task<HourlySummary?> GetByProcessAndHourAsync(ProcessInfo process, DateTime hour);
    }
}
