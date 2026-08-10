using System.Runtime.Versioning;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("linux")]
public class LinuxExecutableMetadataProvider : IExecutableMetadataProvider
{
    public ExecutableMetadata GetMetadata(string executablePath)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
            return new ExecutableMetadata(null, null, null);

        // 1. 尝试找到对应的 .desktop 文件
        string? desktopFilePath = FindDesktopFile(executablePath);
        if (desktopFilePath == null)
        {
            return new ExecutableMetadata(null, null, null);
        }

        // 2. 解析 .desktop 文件中的 Name 和 Icon 字段
        var (name, iconName) = ParseDesktopFile(desktopFilePath);

        if (string.IsNullOrWhiteSpace(iconName))
        {
            return new ExecutableMetadata(name, null, null);
        }

        // 3. 根据图标名称寻找真实的图标文件路径 (支持 PNG / SVG)
        string? iconFilePath = FindIconPath(iconName);
        if (iconFilePath == null || !File.Exists(iconFilePath))
        {
            return new ExecutableMetadata(name, null, null);
        }

        try
        {
            // 4. 读取图标文件数据流
            byte[] bytes = File.ReadAllBytes(iconFilePath);

            // MemoryStream(byte[]) 构造函数默认 Position == 0，可直接交由调用者读取和Dispose
            MemoryStream ms = new(bytes);
            string extension = Path.GetExtension(iconFilePath).TrimStart('.').ToLowerInvariant();

            return new ExecutableMetadata(name, ms, extension);
        }
        catch
        {
            return new ExecutableMetadata(name, null, null);
        }
    }

    /// <summary>
    /// 在系统的 XDG 标准目录中查找与可执行文件匹配的 .desktop 文件
    /// </summary>
    private static string? FindDesktopFile(string executablePath)
    {
        string binaryName = Path.GetFileName(executablePath);
        if (string.IsNullOrEmpty(binaryName))
            return null;

        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // 覆盖原生 Linux 应用、Flatpak、Snap、用户自定义安装的路径
        string[] desktopSearchDirs =
        [
            "/usr/share/applications",
            Path.Combine(home, ".local/share/applications"),
            "/var/lib/snap/desktop/applications",
            "/var/lib/flatpak/exports/share/applications",
            Path.Combine(home, ".local/share/flatpak/exports/share/applications"),
            "/usr/local/share/applications",
        ];

        // 优先级 1: 文件名与二进制名直接匹配 (例如 code.desktop 或 google-chrome.desktop)
        foreach (var dir in desktopSearchDirs)
        {
            if (!Directory.Exists(dir))
                continue;

            string directPath = Path.Combine(dir, $"{binaryName}.desktop");
            if (File.Exists(directPath))
                return directPath;
        }

        // 优先级 2: 扫描 .desktop 文件内容中的 Exec= 行是否包含该二进制路径或名称
        foreach (var dir in desktopSearchDirs)
        {
            if (!Directory.Exists(dir))
                continue;

            try
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.desktop"))
                {
                    if (IsMatchDesktopFile(file, executablePath, binaryName))
                    {
                        return file;
                    }
                }
            }
            catch
            {
                // 忽略没有权限读取的目录
            }
        }

        return null;
    }

    /// <summary>
    /// 检查 .desktop 文件的 Exec 行是否匹配当前执行路径
    /// </summary>
    private static bool IsMatchDesktopFile(
        string desktopFilePath,
        string executablePath,
        string binaryName
    )
    {
        try
        {
            foreach (var line in File.ReadLines(desktopFilePath))
            {
                if (line.StartsWith("Exec=", StringComparison.OrdinalIgnoreCase))
                {
                    string execValue = line[5..].Trim().Trim('"');
                    if (
                        execValue.Contains(executablePath, StringComparison.OrdinalIgnoreCase)
                        || execValue.StartsWith(
                            binaryName + " ",
                            StringComparison.OrdinalIgnoreCase
                        )
                        || execValue.Equals(binaryName, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            // 忽略读取错误
        }
        return false;
    }

    /// <summary>
    /// 从 .desktop 文件提取 [Desktop Entry] 区块下的 Name 和 Icon
    /// </summary>
    private static (string? Name, string? Icon) ParseDesktopFile(string desktopFilePath)
    {
        string? name = null;
        string? icon = null;
        bool inDesktopEntrySection = false;

        try
        {
            foreach (var rawLine in File.ReadLines(desktopFilePath))
            {
                string line = rawLine.Trim();

                if (line.StartsWith('[') && line.EndsWith(']'))
                {
                    inDesktopEntrySection = line.Equals(
                        "[Desktop Entry]",
                        StringComparison.OrdinalIgnoreCase
                    );
                    continue;
                }

                if (!inDesktopEntrySection)
                    continue;

                // 提取默认英文/无语言标记的 Name= (忽略 Name[zh_CN]= 等本地化条目以保证稳定)
                if (name == null && line.StartsWith("Name=", StringComparison.Ordinal))
                {
                    name = line[5..].Trim();
                }
                else if (icon == null && line.StartsWith("Icon=", StringComparison.Ordinal))
                {
                    icon = line[5..].Trim();
                }

                if (name != null && icon != null)
                    break;
            }
        }
        catch
        {
            // 忽略读取错误
        }

        return (name, icon);
    }

    /// <summary>
    /// 从 Linux 图标库中根据 Icon 名称检索真实的图片文件路径
    /// </summary>
    private static string? FindIconPath(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
            return null;

        // 1. 如果 Icon 字段本身就是绝对路径 (例如 Icon=/usr/share/pixmaps/app.png)
        if (Path.IsPathRooted(iconName) && File.Exists(iconName))
        {
            return iconName;
        }

        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string[] iconSearchDirs =
        [
            "/usr/share/icons/hicolor",
            "/usr/share/pixmaps",
            Path.Combine(home, ".local/share/icons/hicolor"),
            "/var/lib/flatpak/exports/share/icons/hicolor",
            "/var/lib/snap/desktop/icons",
        ];

        string[] extensions = [".png", ".svg", ".xpm"];

        // 2. 检查 /usr/share/pixmaps/iconName.png 等简单路径
        foreach (var dir in iconSearchDirs)
        {
            if (!Directory.Exists(dir))
                continue;

            foreach (var ext in extensions)
            {
                string path = Path.Combine(dir, iconName + ext);
                if (File.Exists(path))
                    return path;
            }
        }

        // 3. 递归搜索 hicolor 主题子目录 (如 256x256/apps/iconName.png, scalable/apps/iconName.svg)
        foreach (var dir in iconSearchDirs)
        {
            if (!Directory.Exists(dir))
                continue;

            try
            {
                var files = Directory.EnumerateFiles(
                    dir,
                    $"{iconName}.*",
                    SearchOption.AllDirectories
                );

                // 优先选择 PNG (清晰度高/兼容好)，其次选择 256/512/scalable 尺寸
                var bestMatch = files
                    .Where(f => extensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                    .OrderByDescending(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .ThenByDescending(f =>
                        f.Contains("256") || f.Contains("512") || f.Contains("scalable")
                    )
                    .FirstOrDefault();

                if (bestMatch != null)
                    return bestMatch;
            }
            catch
            {
                // 忽略无权限枚举的文件夹
            }
        }

        return null;
    }
}
