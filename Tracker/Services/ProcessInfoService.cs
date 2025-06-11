using System.Diagnostics;
using Data;
using Microsoft.Extensions.Logging;
using Tracker.Services.Interfaces;

namespace Tracker.Services
{
    internal class ProcessInfoService(ScreenTimeContext context,
        IIconService iconService,
        ILogger<ProcessInfoService> logger) : IProcessInfoService
    {
        private readonly ScreenTimeContext _context = context;
        private readonly IIconService _iconService = iconService;
        private readonly ILogger<ProcessInfoService> _logger = logger;

        public async Task EnsureProcessInfoAsync(Process process)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            string processName = process.ProcessName;
            var processInfo = await _context.ProcessInfos.FindAsync(processName);

            // 如果找不到对应的进程信息，则添加；若最后更新日期不是今天，则覆盖
            if (processInfo == null || processInfo.LastUpdated != today)
            {
                string? exePath = null;
                try { exePath = process.MainModule?.FileName; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get MainModule from process **{ProcessName}**", processName);
                }

                string? iconPath = exePath == null ? null : _iconService.SaveIcon(exePath, processName);
                string? description = exePath == null ? null : FileVersionInfo.GetVersionInfo(exePath).FileDescription;

                if (processInfo == null)
                {
                    // 添加新记录
                    _context.ProcessInfos.Add(new()
                    {
                        ProcessName = processName,
                        ExecutablePath = exePath,
                        IconPath = iconPath,
                        Description = description,
                        LastUpdated = today
                    });
                }
                else if (processInfo.LastUpdated != today)
                {
                    // 更新现有记录
                    processInfo.ExecutablePath = exePath;
                    processInfo.IconPath = iconPath;
                    processInfo.Description = description;
                    processInfo.LastUpdated = today;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
