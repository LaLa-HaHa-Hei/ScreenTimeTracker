using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using ScreenTimeTracker.ScreenTime.Features.Tracking.TrackAppUsage;

namespace ScreenTimeTracker.ScreenTime.Infrastructure.OS;

[SupportedOSPlatform("windows")]
public class WindowsExecutableMetadataProvider : IExecutableMetadataProvider
{
    public ExecutableMetadata GetMetadata(string executablePath)
    {
        string? description = FileVersionInfo.GetVersionInfo(executablePath)?.FileDescription;
        string? productName = FileVersionInfo.GetVersionInfo(executablePath)?.ProductName;
        string? name = description ?? productName;
        using Icon? icon = Icon.ExtractAssociatedIcon(executablePath);
        if (icon is null)
            return new ExecutableMetadata(name, null, null);
        using Bitmap? bmp = icon.ToBitmap();
        if (bmp is null)
            return new ExecutableMetadata(name, null, null);

        // 不能用 using，交给调用者使用和释放
        MemoryStream ms = new();
        bmp.Save(ms, ImageFormat.Png); // 写完数据后会导致 ms.Position == ms.Length
        ms.Position = 0; // 重置 Position 到开头，方便调用者读取数据

        return new ExecutableMetadata(name, ms, "png");
    }
}
