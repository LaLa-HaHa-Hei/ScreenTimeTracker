using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScreenTimeTracker.Application.Configuration;
using ScreenTimeTracker.Application.DTOs;
using ScreenTimeTracker.Application.Interfaces;
using ScreenTimeTracker.Domain.Entities;
using ScreenTimeTracker.Domain.Interfaces;
using System.ComponentModel;
using System.Diagnostics;


namespace ScreenTimeTracker.Application.Services
{
    public class ProcessManagementService(ILogger<ProcessManagementService> logger, IProcessInfoRepository processInfoRepository, IExecutableMetadataProvider processInfoProvider, IOptions<TrackerOptions> trackerOptions)
    {
        private readonly ILogger<ProcessManagementService> _logger = logger;
        private readonly IProcessInfoRepository _processInfoRepository = processInfoRepository;
        private readonly IExecutableMetadataProvider _executableMetadataProvider = processInfoProvider;
        private readonly IOptions<TrackerOptions> _trackerOptions = trackerOptions;

        private async Task UpdateProcessInfoAsync(ProcessInfo processInfo, string? executablePath)
        {
            string? iconPath = null;
            string? description = null;
            if (executablePath is not null)
            {
                ExecutableMetadata metadata = await _executableMetadataProvider.GetMetadataAsync(executablePath);
                description = metadata.Description;
                if (metadata.IconBytes is not null)
                {
                    string directory = Path.Combine("Data", "Icons");
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    iconPath = Path.Combine(directory, $"{processInfo.Name}.{metadata.FileExtension}");
                    await File.WriteAllBytesAsync(iconPath, metadata.IconBytes);
                }
            }
            processInfo.UpdateSystemDetails(executablePath, iconPath, description);
        }

        private string? GetExecutablePath(Process? process)
        {
            try
            {
                return process?.MainModule?.FileName;
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
            {
                _logger.LogWarning(ex, "Unable to access information for process {ProcessName}.", process?.ProcessName ?? ProcessInfo.UnknownProcessName);
                return null;
            }
        }

        public async Task<ProcessInfo> EnsureUnknownProcessInfoExistsAsync()
        {
            ProcessInfo? existing = await _processInfoRepository.GetByIdAsync(ProcessInfo.UnknownProcessId);
            if (existing is not null)
                return existing;

            ProcessInfo processInfo = ProcessInfo.Reconstitute(
                id: ProcessInfo.UnknownProcessId,
                name: ProcessInfo.UnknownProcessName,
                alias: null,
                autoUpdate: false,
                lastAutoUpdated: DateTime.Now,
                executablePath: null,
                iconPath: null,
                description: null);
            await _processInfoRepository.AddAsync(processInfo);
            return processInfo;
        }

        public async Task<ProcessInfo> EnsureIdleProcessInfoExistsAsync()
        {
            ProcessInfo? existing = await _processInfoRepository.GetByIdAsync(ProcessInfo.IdleProcessId);
            if (existing is not null)
                return existing;

            ProcessInfo processInfo = ProcessInfo.Reconstitute(
                id: ProcessInfo.IdleProcessId,
                name: ProcessInfo.IdleProcessName,
                alias: null,
                autoUpdate: false,
                lastAutoUpdated: DateTime.Now,
                executablePath: null,
                iconPath: null,
                description: null);
            await _processInfoRepository.AddAsync(processInfo);
            return processInfo;
        }

        public async Task<ProcessInfo> EnsureProcessInfoExistsAsync(Process? process)
        {
            string name = process?.ProcessName ?? ProcessInfo.UnknownProcessName;
            if (name is null)
                return await EnsureUnknownProcessInfoExistsAsync();

            ProcessInfo? existing = await _processInfoRepository.GetByNameAsync(name);

            // 已有记录
            if (existing is not null)
            {
                // 检查是否需要更新
                if (existing.AutoUpdate && (DateTime.Now - existing.LastAutoUpdated) > TimeSpan.FromMinutes(_trackerOptions.Value.ProcessInfoStaleThresholdMinutes))
                {
                    await UpdateProcessInfoAsync(existing, GetExecutablePath(process));
                    await _processInfoRepository.UpdateAsync(existing);
                }
                return existing;
            }

            // 无记录，创建新记录
            ProcessInfo processInfo = new(Guid.NewGuid(), name);
            await UpdateProcessInfoAsync(processInfo, GetExecutablePath(process));
            await _processInfoRepository.AddAsync(processInfo);
            return processInfo;
        }
    }
}
