using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ExportData;

public partial class ExportDataHandler(
    ILogger<ExportDataHandler> logger,
    ScreenTimeDbContext context
) : IRequestHandler<ExportDataQuery, ExportDataResponse>
{
    public async ValueTask<ExportDataResponse> Handle(
        ExportDataQuery request,
        CancellationToken cancellationToken
    )
    {
        // App

        var apps = await context
            .Apps.Join(
                context.AppCategories,
                app => app.AppCategoryId,
                category => category.Id,
                (app, category) =>
                    new
                    {
                        app.Name,
                        app.Color,
                        app.ProcessName,
                        app.AllowMetadataAutoRefresh,
                        CategoryName = category.Name,
                        app.IconPath,
                    }
            )
            .ToListAsync(cancellationToken);

        ExportDataResponse.App[] appResults;
        using (var semaphore = new SemaphoreSlim(10))
        {
            appResults = await Task.WhenAll(
                apps.Select(async x =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return new ExportDataResponse.App(
                            x.Name,
                            x.Color,
                            x.ProcessName,
                            x.AllowMetadataAutoRefresh,
                            x.CategoryName,
                            await GetIconAsync(x.IconPath, cancellationToken)
                        );
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }

        var appCategories = await context
            .AppCategories.Select(x => new
            {
                x.Name,
                x.Color,
                x.IconPath,
            })
            .ToListAsync(cancellationToken);

        ExportDataResponse.AppCategory[] appCategoryResults;
        using (var semaphore = new SemaphoreSlim(10))
        {
            appCategoryResults = await Task.WhenAll(
                appCategories.Select(async x =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return new ExportDataResponse.AppCategory(
                            x.Name,
                            x.Color,
                            await GetIconAsync(x.IconPath, cancellationToken)
                        );
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }

        var appUsageSessions = await context
            .AppUsageSessions.Join(
                context.Apps,
                session => session.AppId,
                app => app.Id,
                (session, app) =>
                    new ExportDataResponse.AppUsageSession(
                        app.ProcessName,
                        session.UsagePeriod.Start,
                        session.UsagePeriod.End
                    )
            )
            .ToListAsync(cancellationToken);

        // Website

        var websites = await context
            .Websites.Join(
                context.WebsiteCategories,
                website => website.WebsiteCategoryId,
                category => category.Id,
                (website, category) =>
                    new
                    {
                        website.Name,
                        website.Color,
                        website.Host,
                        website.AllowMetadataAutoRefresh,
                        CategoryName = category.Name,
                        website.IconPath,
                    }
            )
            .ToListAsync(cancellationToken);

        ExportDataResponse.Website[] websiteResults;
        using (var semaphore = new SemaphoreSlim(10))
        {
            websiteResults = await Task.WhenAll(
                websites.Select(async x =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return new ExportDataResponse.Website(
                            x.Name,
                            x.Color,
                            x.Host,
                            x.AllowMetadataAutoRefresh,
                            x.CategoryName,
                            await GetIconAsync(x.IconPath, cancellationToken)
                        );
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }

        var websiteCategories = await context
            .WebsiteCategories.Select(x => new
            {
                x.Name,
                x.Color,
                x.IconPath,
            })
            .ToListAsync(cancellationToken);

        ExportDataResponse.WebsiteCategory[] websiteCategoryResults;
        using (var semaphore = new SemaphoreSlim(10))
        {
            websiteCategoryResults = await Task.WhenAll(
                websiteCategories.Select(async x =>
                {
                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        return new ExportDataResponse.WebsiteCategory(
                            x.Name,
                            x.Color,
                            await GetIconAsync(x.IconPath, cancellationToken)
                        );
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                })
            );
        }

        var websiteUsageSessions = await context
            .WebsiteUsageSessions.Join(
                context.Websites,
                session => session.WebsiteId,
                website => website.Id,
                (session, website) =>
                    new ExportDataResponse.WebsiteUsageSession(
                        website.Host,
                        session.UsagePeriod.Start,
                        session.UsagePeriod.End
                    )
            )
            .ToListAsync(cancellationToken);

        return new ExportDataResponse(
            [.. appResults],
            [.. appCategoryResults],
            [.. appUsageSessions],
            [.. websiteResults],
            [.. websiteCategoryResults],
            [.. websiteUsageSessions]
        );
    }

    public async Task<ExportDataResponse.Icon?> GetIconAsync(
        string? iconPath,
        CancellationToken cancellationToken
    )
    {
        if (iconPath is null || (!File.Exists(iconPath)))
            return null;

        try
        {
            string extension = Path.GetExtension(iconPath);
            byte[] fileBytes = await File.ReadAllBytesAsync(iconPath, cancellationToken);
            return new ExportDataResponse.Icon(extension, fileBytes);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            LogReadIconFileFailed(logger, iconPath);
            return null;
        }
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to read icon file at path: {IconPath}"
    )]
    private static partial void LogReadIconFileFailed(ILogger logger, string iconPath);
}
