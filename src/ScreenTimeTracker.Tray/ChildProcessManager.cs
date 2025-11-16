using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.System.JobObjects;

namespace ScreenTimeTracker.Tray
{
#pragma warning disable CA1416 // 验证平台兼容性
    public static class ChildProcessManager
    {
        private static bool _initialized;
        private static SafeFileHandle? _jobHandle;

        private static void Initialize()
        {
            var jobHandle = PInvoke.CreateJobObject(default, null);
            if (jobHandle.IsInvalid)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "无法创建 Job Object");
            }
            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE,
                }
            };
            var infoBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref extendedInfo, 1));

            if (!PInvoke.SetInformationJobObject(jobHandle, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, infoBytes))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "无法设置 Job Object 信息");
            }

            _jobHandle = jobHandle;
            _initialized = true;
        }

        public static void AddProcess(Process process)
        {
            if (!_initialized)
            {
                Initialize();
            }
            if (!PInvoke.AssignProcessToJobObject(_jobHandle, process.SafeHandle))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "无法将进程添加到 Job Object");
            }
        }
    }
#pragma warning restore CA1416 // 验证平台兼容性
}