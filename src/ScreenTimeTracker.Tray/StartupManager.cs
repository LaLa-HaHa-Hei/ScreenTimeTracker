using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;

namespace ScreenTimeTracker.Tray
{
#pragma warning disable CA1416 // 验证平台兼容性
    public static class StartupManager
    {
        private const string RegistryRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        private static bool IsRunningAsAdmin()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsStartupEnabled(string appName)
        {
            return IsTaskSchedulerStartupEnabled(appName) || IsRegistryStartupEnabled(appName);
        }

        public static void EnableStartup(string appName, string filePath)
        {
            if (IsRunningAsAdmin())
                EnableTaskSchedulerStartup(appName, filePath);
            else
                EnableRegistryStartup(appName, filePath);
        }

        public static void DisableStartup(string appName)
        {
            if (IsRunningAsAdmin() && IsTaskSchedulerStartupEnabled(appName))
                DisableTaskSchedulerStartup(appName);
            if (IsRegistryStartupEnabled(appName))
                DisableRegistryStartup(appName);
        }

        private static bool IsRegistryStartupEnabled(string appName)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, false);
            return key?.GetValue(appName) != null;
        }

        private static void EnableRegistryStartup(string appName, string filePath)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, true);
            key?.SetValue(appName, $"\"{filePath}\"");
        }

        private static void DisableRegistryStartup(string appName)
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, true);
            key?.DeleteValue(appName, false); // false表示如果值不存在也不会抛出异常
        }

        private static bool IsTaskSchedulerStartupEnabled(string appName)
        {
            using var ts = new TaskService();
            return ts.FindTask(appName) != null;
        }

        private static void EnableTaskSchedulerStartup(string appName, string filePath)
        {
            using var ts = new TaskService();
            var td = ts.NewTask();
            td.RegistrationInfo.Description = $"Starts {appName} on user logon.";
            td.RegistrationInfo.Author = "ScreenTimeTracker.Tray.StartupManager";

            td.Principal.RunLevel = TaskRunLevel.Highest;

            // 创建一个在用户登录时触发的触发器
            td.Triggers.Add(new LogonTrigger());

            // 创建启动应用程序的操作
            td.Actions.Add(new ExecAction(filePath));

            // 注册任务
            ts.RootFolder.RegisterTaskDefinition(appName, td);
        }

        private static void DisableTaskSchedulerStartup(string appName)
        {
            using var ts = new TaskService();
            ts.RootFolder.DeleteTask(appName, false);
        }
    }
#pragma warning restore CA1416 // 验证平台兼容性
}
