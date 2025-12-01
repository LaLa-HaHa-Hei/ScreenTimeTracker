using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using ScreenTimeTracker.ApplicationLayer.Common.Interfaces;
using System.Security.Principal;

namespace ScreenTimeTracker.Infrastructure.OS.Windows;

#pragma warning disable CA1416 // 验证平台兼容性
public class StartupManager : IStartupManager
{
    private const string RegistryRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    private static bool IsRunningAsAdmin()
    {
        var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public bool IsStartupEnabled(string appName)
    {
        return IsTaskSchedulerStartupEnabled(appName) || IsRegistryStartupEnabled(appName);
    }

    public void EnableStartup(string appName, string filePath)
    {
        if (IsRunningAsAdmin())
            EnableTaskSchedulerStartup(appName, filePath);
        else
            EnableRegistryStartup(appName, filePath);
    }

    public void DisableStartup(string appName)
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
        td.RegistrationInfo.Author = typeof(StartupManager).FullName;
        // 以最高权限运行
        td.Principal.RunLevel = TaskRunLevel.Highest;
        // 创建一个在用户登录时触发的触发器
        td.Triggers.Add(new LogonTrigger());
        // 创建启动应用程序的操作
        td.Actions.Add(new ExecAction(filePath));
        // 不限制电池情况下启动
        td.Settings.DisallowStartIfOnBatteries = false;
        // 切换到电池时不停止任务  
        td.Settings.StopIfGoingOnBatteries = false;
        // 关闭“如果运行时间超过…，停止任务”
        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
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
