using Data;
using Data.Entities;
using Tracker.Services.Interfaces;

namespace Tracker.Services
{
    internal class UsageAggregator(ScreenTimeContext context) : IUsageAggregator
    {
        private readonly ScreenTimeContext _context = context;
        private readonly Dictionary<(DateOnly date, int hour, string processName), long> _usageDict = [];

        public void AddUsage(string processName, int durationMs)
        {
            var now = DateTime.Now;
            var key = (DateOnly.FromDateTime(now), now.Hour, processName);
            if (!_usageDict.ContainsKey(key))
                _usageDict[key] = 0;
            _usageDict[key] += durationMs;
        }

        public async Task SaveAsync()
        {
            foreach (var ((date, hour, processName), duration) in _usageDict)
            {
                var hourUsage = await _context.HourlyUsages.FindAsync(date, hour, processName);
                if (hourUsage == null)
                {
                    hourUsage = new HourlyUsage { ProcessName = processName, Date = date, Hour = hour, DurationMs = 0 };
                    _context.HourlyUsages.Add(hourUsage);
                }
                hourUsage.DurationMs += duration;

                var dailyUsage = await _context.DailyUsages.FindAsync(date, processName);
                if (dailyUsage == null)
                {
                    dailyUsage = new DailyUsage { ProcessName = processName, Date = date, DurationMs = 0 };
                    _context.DailyUsages.Add(dailyUsage);
                }
                dailyUsage.DurationMs += duration;
            }
            await _context.SaveChangesAsync();
            _usageDict.Clear();
        }
    }
}
