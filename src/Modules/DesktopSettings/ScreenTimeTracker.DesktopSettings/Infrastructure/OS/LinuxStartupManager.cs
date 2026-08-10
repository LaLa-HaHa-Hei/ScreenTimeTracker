using System.Runtime.Versioning;
using ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

namespace ScreenTimeTracker.DesktopSettings.Infrastructure.OS;

[SupportedOSPlatform("linux")]
public class LinuxStartupManager : IStartupManager
{
    private const string AppName = "ScreenTimeTracker";
    private const string DesktopFileName = "ScreenTimeTracker.desktop";
    private readonly bool _isRunningAsRoot = Environment.IsPrivilegedProcess;

    public bool IsEnabled()
    {
        return IsAutostartEnabledForFile(GetUserAutostartFilePath())
            || IsAutostartEnabledForFile(GetSystemAutostartFilePath());
    }

    public void Enable()
    {
        string currentPath =
            Environment.ProcessPath
            ?? throw new InvalidOperationException("Current process path is null.");

        // 如果是 Root 权限运行，则写入系统级全局自启动；否则写入当前用户自启动
        string targetFilePath = _isRunningAsRoot
            ? GetSystemAutostartFilePath()
            : GetUserAutostartFilePath();

        // 构建 XDG 规范的 .desktop 文件内容
        string desktopFileContent = $"""
            [Desktop Entry]
            Type=Application
            Name={AppName}
            Comment=Starts {AppName} on user logon.
            Exec="{currentPath}"
            Terminal=false
            Categories=Utility;
            X-GNOME-Autostart-enabled=true
            """;

        string? directory = Path.GetDirectoryName(targetFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(targetFilePath, desktopFileContent);
    }

    public void Disable()
    {
        // 删除普通用户级别的自启动文件
        DeleteFileIfExists(GetUserAutostartFilePath());

        // 如果是 Root 权限，同步尝试删除系统级的自启动文件
        if (_isRunningAsRoot)
        {
            DeleteFileIfExists(GetSystemAutostartFilePath());
        }
    }

    /// <summary>
    /// 获取当前用户的自启动文件路径 (~/.config/autostart/ScreenTimeTracker.desktop)
    /// </summary>
    private static string GetUserAutostartFilePath()
    {
        string xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")!;
        if (string.IsNullOrWhiteSpace(xdgConfigHome))
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            xdgConfigHome = Path.Combine(homeDir, ".config");
        }

        return Path.Combine(xdgConfigHome, "autostart", DesktopFileName);
    }

    /// <summary>
    /// 获取系统全局的自启动文件路径 (/etc/xdg/autostart/ScreenTimeTracker.desktop)
    /// </summary>
    private static string GetSystemAutostartFilePath()
    {
        return Path.Combine("/etc", "xdg", "autostart", DesktopFileName);
    }

    /// <summary>
    /// 检查指定的 .desktop 配置文件是否存在且未被显式禁用
    /// </summary>
    private static bool IsAutostartEnabledForFile(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                // 检查用户是否在系统设置菜单中手动关闭了自启动（桌面系统会向 .desktop 写入这两行之一）
                if (
                    trimmed.Equals("Hidden=true", StringComparison.OrdinalIgnoreCase)
                    || trimmed.Equals(
                        "X-GNOME-Autostart-enabled=false",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return false;
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void DeleteFileIfExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // 忽略没有权限或删除失败的异常
            }
        }
    }
}
