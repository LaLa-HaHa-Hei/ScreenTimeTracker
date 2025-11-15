using ScreenTimeTracker.Application.DTOs;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Infrastructure.Persistence.DbContexts;

namespace ScreenTimeTracker.Infrastructure.Persistence.Queries
{
    class UsageReportQueries(ScreenTimeDbContext dbContext) : IUsageReportQueries
    {
        private readonly ScreenTimeDbContext _dbContext = dbContext;

        public Task<IDictionary<DateOnly, TimeSpan>> GetProcessDailyDistributionForPeriodAsync(DateOnly startDate, Guid processId)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<DateOnly, TimeSpan>> GetProcessDailyDistributionForPeriodAsync(DateOnly startDate, DateOnly endDate, Guid processId)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<int, TimeSpan>> GetProcessHourlyDistributionForDayAsync(DateOnly date, Guid processId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProcessUsageRankEntry>> GetRankedProcessUsageForDayAsync(DateOnly date)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProcessUsageRankEntry>> GetRankedProcessUsageForPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<DateOnly, TimeSpan>> GetTotalDailyUsageForPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            throw new NotImplementedException();
        }

        public Task<IDictionary<int, TimeSpan>> GetTotalHourlyUsageForDayAsync(DateOnly date)
        {
            throw new NotImplementedException();
        }
    }
}
