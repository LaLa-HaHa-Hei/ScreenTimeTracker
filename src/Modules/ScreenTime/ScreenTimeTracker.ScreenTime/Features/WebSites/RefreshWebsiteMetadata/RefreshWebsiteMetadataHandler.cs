using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.UserSettings;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
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

        var iconPath = await EnsureIconUpdated(website, request.Icon, settings);

        website.RefreshMetadata(timeProvider.GetUtcNow(), iconPath);

        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }

    private static async Task<string?> EnsureIconUpdated(
        Website website,
        Icon? icon,
        UserSettings settings
    )
    {
        if (icon is null)
        {
            if (File.Exists(website.IconPath))
                File.Delete(website.IconPath);
            return null;
        }

        // 如果新旧图标大小相同，跳过 IO 操作（简单校验）
        if (
            File.Exists(website.IconPath)
            && new FileInfo(website.IconPath).Length == icon.Data.Length
        )
        {
            return website.IconPath;
        }

        // 确保目录存在
        Directory.CreateDirectory(settings.WebsiteTracking.IconDirectory);

        string newIconPath = Path.Combine(
            settings.WebsiteTracking.IconDirectory,
            $"{website.Host.Replace(':', '-')}.{icon.Extension}"
        );

        // 使用 FileMode.Create 自动覆盖旧文件
        await using (
            var fileStream = new FileStream(
                newIconPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            )
        )
        {
            await fileStream.WriteAsync(icon.Data);
        }

        return newIconPath;
    }
}
