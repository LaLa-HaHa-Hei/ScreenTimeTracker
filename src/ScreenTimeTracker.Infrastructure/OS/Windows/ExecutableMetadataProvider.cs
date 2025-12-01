using ScreenTimeTracker.ApplicationLayer.Common.DTOs;
using ScreenTimeTracker.ApplicationLayer.Common.Interfaces;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

namespace ScreenTimeTracker.Infrastructure.OS.Windows;

#pragma warning disable CA1416 // Validate platform compatibility
public class ExecutableMetadataProvider : IExecutableMetadataProvider
{
    public Task<ExecutableMetadata> GetMetadataAsync(string exePath)
    {
        string? description = FileVersionInfo.GetVersionInfo(exePath)?.FileDescription;
        using Icon? icon = Icon.ExtractAssociatedIcon(exePath);
        if (icon is null)
            return Task.FromResult(new ExecutableMetadata(description, null, null));
        using Bitmap? bmp = icon.ToBitmap();
        if (bmp is null)
            return Task.FromResult(new ExecutableMetadata(description, null, null));

        using MemoryStream ms = new();
        bmp.Save(ms, ImageFormat.Png);

        byte[] pngBytes = ms.ToArray();

        return Task.FromResult(new ExecutableMetadata(
            description,
            pngBytes,
            "png"
        ));
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
