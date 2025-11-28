using ScreenTimeTracker.DomainLayer.Entities;

namespace ScreenTimeTracker.DomainLayer.Interfaces
{
    public interface IHourlySummaryRepository
    {
        Task AddAsync(HourlySummary hourlySummary);
        Task UpdateAsync(HourlySummary hourlySummary);
        Task<HourlySummary?> GetByProcessAndHourAsync(ProcessInfo process, DateTime hour);
    }
}
