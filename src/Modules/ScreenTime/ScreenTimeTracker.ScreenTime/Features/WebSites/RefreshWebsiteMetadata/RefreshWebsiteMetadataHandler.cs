using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.RefreshWebsiteMetadata;

public class RefreshWebsiteMetadataHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<RefreshWebsiteMetadataCommand, ErrorOr<Updated>>
{
    public async ValueTask<ErrorOr<Updated>> Handle(
        RefreshWebsiteMetadataCommand request,
        CancellationToken cancellationToken
    )
    {
        Website? website = await context.Websites.FindAsync([request.Id], cancellationToken);
        if (website is null)
            return Error.NotFound(
                code: "Website.NotFound",
                description: "The website with the specified ID was not found."
            );

        var settings = await context.UserSettings.SingleAsync(cancellationToken);

        var iconPath = await EnsureIconUpdated(
            website,
            request.Icon,
            settings.WebsiteTracking.IconDirectory
        );

        website.RefreshMetadata(timeProvider.GetUtcNow(), iconPath);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }

    private static async Task<string?> EnsureIconUpdated(
        Website website,
        Icon? icon,
        string iconDir
    )
    {
        if (icon is null)
        {
            CleanUpManagedIcon(website.IconPath, iconDir);
            return null;
        }

        string newIconPath = Path.Combine(
            iconDir,
            $"{website.Host.Replace(':', '-')}.{icon.Extension}"
        );

        StringComparison pathComparison =
            OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
        bool isSamePath = string.Equals(website.IconPath, newIconPath, pathComparison);

        // 如果新旧路径不一致，且旧图标属于受管理目录，则清理旧图标
        if (!isSamePath)
            CleanUpManagedIcon(website.IconPath, iconDir);
        // 简单校验，如果路径一致且新旧图标大小相同，跳过 IO 操作
        else if (File.Exists(newIconPath) && new FileInfo(newIconPath).Length == icon.Data.Length)
            return newIconPath;

        // 确保目录存在
        Directory.CreateDirectory(iconDir);

        // 使用 FileMode.Create 自动覆盖旧文件
        await using (
            var fileStream = new FileStream(
                newIconPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            )
        )
            await fileStream.WriteAsync(icon.Data);

        return newIconPath;
    }

    /// 仅清理在受管理目录（managedDirectory）下的旧文件
    private static void CleanUpManagedIcon(string? filePath, string managedDirectory)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        // 只有文件确实在软件管理的图标目录下时，才允许删除
        if (IsFileInDirectory(filePath, managedDirectory))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // 忽略删除失败的异常（如文件被占用）
            }
        }
    }

    // 判断文件路径是否位于指定目录下
    private static bool IsFileInDirectory(string filePath, string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(directoryPath))
            return false;

        try
        {
            // 获取相对路径（Path.GetRelativePath 会自动处理 Windows/macOS/Linux 的大小写规则）
            string relativePath = Path.GetRelativePath(directoryPath, filePath);

            // 满足以下条件说明 filePath 确实在 directoryPath 目录（或子目录）下：
            //    - 相对路径不等于 "." (代表就是目录本身)
            //    - 相对路径不以 ".." 开头 (代表不在父级或平级目录)
            //    - 相对路径不是根绝对路径 (代表没有跨驱动器，比如从 C:\ 到 D:\)
            return relativePath != "."
                && !relativePath.StartsWith("..", StringComparison.Ordinal)
                && !Path.IsPathRooted(relativePath);
        }
        catch
        {
            return false;
        }
    }
}
