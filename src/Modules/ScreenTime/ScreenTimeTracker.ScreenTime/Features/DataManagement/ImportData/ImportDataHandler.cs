using System.Text.Json;
using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Apps;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteCategories;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteUsageSessions;
using ScreenTimeTracker.ScreenTime.Domain.ValueObjects;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.DataManagement.ImportData;

public class ImportDataHandler(
    ScreenTimeDbContext context,
    TimeProvider timeProvider,
    ActiveAppUsageSessionStore activeAppUsageSessionStore,
    ActiveWebsiteUsageSessionStore activeWebsiteUsageSessionStore
) : IRequestHandler<ImportDataCommand, ErrorOr<ImportDataResponse>>
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async ValueTask<ErrorOr<ImportDataResponse>> Handle(
        ImportDataCommand request,
        CancellationToken cancellationToken
    )
    {
        int version;
        try
        {
            using var doc = JsonDocument.Parse(request.RawJson);

            if (!doc.RootElement.TryGetProperty("version", out var versionElement))
            {
                return Error.Validation(
                    "ImportData.MissingVersion",
                    "The imported data is missing the version field."
                );
            }

            if (!versionElement.TryGetInt32(out version))
            {
                return Error.Validation(
                    "ImportData.InvalidVersion",
                    "The data version must be an integer."
                );
            }
        }
        catch (JsonException)
        {
            return Error.Validation(
                "ImportData.InvalidJson",
                "The imported data is not valid JSON."
            );
        }

        ImportDataContracts.V4Data v4Data;
        try
        {
            switch (version)
            {
                case 1:
                    v4Data = UpcastV3ToV4(
                        UpcastV2ToV3(
                            UpcastV1ToV2(Deserialize<ImportDataContracts.V1Data>(request.RawJson))
                        )
                    );
                    break;
                case 2:
                    v4Data = UpcastV3ToV4(
                        UpcastV2ToV3(Deserialize<ImportDataContracts.V2Data>(request.RawJson))
                    );
                    break;
                case 3:
                    v4Data = UpcastV3ToV4(Deserialize<ImportDataContracts.V3Data>(request.RawJson));
                    break;
                case 4:
                    v4Data = Deserialize<ImportDataContracts.V4Data>(request.RawJson);
                    break;
                default:
                    return Error.Validation(
                        "ImportData.UnsupportedVersion",
                        $"Unsupported data version: {version}"
                    );
            }
        }
        catch (JsonException)
        {
            return Error.Validation(
                "ImportData.DeserializationFailed",
                "The imported data could not be parsed because its structure does not match the expected format."
            );
        }

        return await SaveDataAsync(v4Data, cancellationToken);
    }

    private static T Deserialize<T>(string json)
    {
        var result = JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);

        return result ?? throw new JsonException("Deserialized data is null.");
    }

    private static class ImportDataContracts
    {
        public sealed record Icon(string Extension, byte[] Data);

        // V1 数据结构
        public sealed record V1Data(V1UsageSession[] UsageSessions);

        public sealed record V1UsageSession(
            string AppName,
            string AppProcessName,
            DateTime StartTime,
            DateTime EndTime
        );

        // V2 数据结构
        public sealed record V2Data(
            V2AppCategory[] AppCategories,
            V2App[] Apps,
            V2AppUsageSession[] AppUsageSessions
        );

        public sealed record V2AppCategory(string Name, Icon? Icon);

        public sealed record V2App(
            string Name,
            string ProcessName,
            string AppCategoryName,
            Icon? Icon
        );

        public sealed record V2AppUsageSession(
            string AppProcessName,
            DateTime StartTime,
            DateTime EndTime
        );

        // V3 数据结构
        public sealed record V3Data(
            V3AppCategory[] AppCategories,
            V3App[] Apps,
            V3AppUsageSession[] AppUsageSessions
        );

        public sealed record V3AppCategory(string Name, string Color, Icon? Icon);

        public sealed record V3App(
            string Name,
            string Color,
            string ProcessName,
            bool IsAutoRefreshEnabled,
            string AppCategoryName,
            Icon? Icon
        );

        public sealed record V3AppUsageSession(
            string AppProcessName,
            DateTime StartTime,
            DateTime EndTime
        );

        // V4 数据结构
        public sealed record V4Data(
            V4App[] Apps,
            V4AppCategory[] AppCategories,
            V4AppUsageSession[] AppUsageSessions,
            V4Website[] Websites,
            V4WebsiteCategory[] WebsiteCategories,
            V4WebsiteUsageSession[] WebsiteUsageSessions
        );

        public sealed record V4App(
            string Name,
            string Color,
            string ProcessName,
            bool AllowMetadataAutoRefresh,
            string AppCategoryName,
            Icon? Icon
        );

        public sealed record V4AppCategory(string Name, string Color, Icon? Icon);

        public sealed record V4AppUsageSession(
            string AppProcessName,
            DateTimeOffset StartTime,
            DateTimeOffset EndTime
        );

        public sealed record V4Website(
            string Name,
            string Color,
            string Host,
            bool AllowMetadataAutoRefresh,
            string WebsiteCategoryName,
            Icon? Icon
        );

        public sealed record V4WebsiteCategory(string Name, string Color, Icon? Icon);

        public sealed record V4WebsiteUsageSession(
            string WebsiteHost,
            DateTimeOffset StartTime,
            DateTimeOffset EndTime
        );
    }

    private static ImportDataContracts.V2Data UpcastV1ToV2(ImportDataContracts.V1Data v1)
    {
        var apps = v1
            .UsageSessions.DistinctBy(s => s.AppProcessName)
            .Select(s => new ImportDataContracts.V2App(
                s.AppName,
                s.AppProcessName,
                "Uncategorized",
                null
            ))
            .ToArray();

        var sessions = v1
            .UsageSessions.Select(s => new ImportDataContracts.V2AppUsageSession(
                s.AppProcessName,
                s.StartTime,
                s.EndTime
            ))
            .ToArray();

        return new ImportDataContracts.V2Data([], apps, sessions);
    }

    private static ImportDataContracts.V3Data UpcastV2ToV3(ImportDataContracts.V2Data v2)
    {
        var categories = v2
            .AppCategories.Select(c => new ImportDataContracts.V3AppCategory(
                c.Name,
                App.GenerateColor(),
                c.Icon
            ))
            .ToArray();

        var apps = v2
            .Apps.Select(a => new ImportDataContracts.V3App(
                a.Name,
                App.GenerateColor(),
                a.ProcessName,
                true,
                a.AppCategoryName,
                a.Icon
            ))
            .ToArray();

        var sessions = v2
            .AppUsageSessions.Select(s => new ImportDataContracts.V3AppUsageSession(
                s.AppProcessName,
                s.StartTime,
                s.EndTime
            ))
            .ToArray();

        return new ImportDataContracts.V3Data(categories, apps, sessions);
    }

    private static ImportDataContracts.V4Data UpcastV3ToV4(ImportDataContracts.V3Data v3)
    {
        var apps = v3
            .Apps.Select(a => new ImportDataContracts.V4App(
                a.Name,
                a.Color,
                a.ProcessName,
                true,
                a.AppCategoryName,
                a.Icon
            ))
            .ToArray();

        var appCategories = v3
            .AppCategories.Select(c => new ImportDataContracts.V4AppCategory(
                c.Name,
                c.Color,
                c.Icon
            ))
            .ToArray();

        // 之前版本的时间存储是采用的地时间，在此之后的则采用UTC时间
        var appUsageSessions = v3
            .AppUsageSessions.Select(s => new ImportDataContracts.V4AppUsageSession(
                s.AppProcessName,
                new DateTimeOffset(
                    DateTime.SpecifyKind(s.StartTime, DateTimeKind.Local)
                ).UtcDateTime,
                new DateTimeOffset(DateTime.SpecifyKind(s.EndTime, DateTimeKind.Local)).UtcDateTime
            ))
            .ToArray();

        return new ImportDataContracts.V4Data(apps, appCategories, appUsageSessions, [], [], []);
    }

    private async Task<ImportDataResponse> SaveDataAsync(
        ImportDataContracts.V4Data data,
        CancellationToken cancellationToken
    )
    {
        if (
            data.AppCategories.Length == 0
            && data.Apps.Length == 0
            && data.AppUsageSessions.Length == 0
            && data.Websites.Length == 0
            && data.WebsiteCategories.Length == 0
            && data.WebsiteUsageSessions.Length == 0
        )
            return new ImportDataResponse(0, 0, 0, 0, 0, 0, 0, 0);

        long newApps = 0;
        long newAppCategories = 0;
        long importedAppUsageSessions = 0;
        long skippedAppUsageSessions = 0;
        long newWebsites = 0;
        long newWebsiteCategories = 0;
        long importedWebsiteUsageSessions = 0;
        long skippedWebsiteUsageSessions = 0;

        DateTimeOffset now = timeProvider.GetUtcNow();
        var userSettings = await context.UserSettings.SingleAsync(cancellationToken);
        var appIconDirectory = userSettings.AppTracking.IconDirectory;
        var appCategoryIconDirectory = "./Data/AppCategoryIcons";
        var websiteIconDirectory = userSettings.WebsiteTracking.IconDirectory;
        var websiteCategoryIconDirectory = "./Data/WebsiteCategoryIcons";

        {
            // AppCategory
            var existingCategories = await context.AppCategories.ToDictionaryAsync(
                c => c.Name,
                cancellationToken
            );

            foreach (var categoryData in data.AppCategories)
            {
                if (!existingCategories.TryGetValue(categoryData.Name, out var category))
                {
                    var iconPath = await SaveIconAsync(
                        categoryData.Icon,
                        categoryData.Name,
                        appCategoryIconDirectory,
                        cancellationToken
                    );
                    category = AppCategory.Import(categoryData.Name, categoryData.Color, iconPath);
                    context.AppCategories.Add(category);
                    existingCategories[categoryData.Name] = category;
                    newAppCategories++;
                }
            }

            // Apps
            var existingApps = await context.Apps.ToDictionaryAsync(
                a => a.ProcessName,
                cancellationToken
            );

            foreach (var appData in data.Apps)
            {
                if (!existingApps.TryGetValue(appData.ProcessName, out var app))
                {
                    var iconPath = await SaveIconAsync(
                        appData.Icon,
                        appData.Name,
                        appIconDirectory,
                        cancellationToken
                    );
                    existingCategories.TryGetValue(
                        appData.AppCategoryName,
                        out var matchedCategory
                    );
                    if (matchedCategory is null)
                    {
                        matchedCategory = AppCategory.Import(
                            appData.AppCategoryName,
                            App.GenerateColor(),
                            null
                        );
                        context.AppCategories.Add(matchedCategory);
                        existingCategories[appData.AppCategoryName] = matchedCategory;
                        newAppCategories++;
                    }
                    app = App.Import(
                        appData.Name,
                        appData.Color,
                        appData.ProcessName,
                        appData.AllowMetadataAutoRefresh,
                        matchedCategory.Id,
                        iconPath
                    );

                    context.Apps.Add(app);
                    existingApps[appData.ProcessName] = app;
                    newApps++;
                }
            }

            // AppUsageSessions
            if (data.AppUsageSessions.Length != 0)
            {
                var minStart = data.AppUsageSessions.Min(s => s.StartTime);
                var maxEnd = data.AppUsageSessions.Max(s => s.EndTime);

                var existingSessions = await context
                    .AppUsageSessions.Where(s => minStart < s.EndTime && s.StartTime < maxEnd)
                    .ToListAsync(cancellationToken);

                var activeSession = activeAppUsageSessionStore.Current;
                if (activeSession is not null && activeSession.StartTime < maxEnd && minStart < now)
                    existingSessions.Add(
                        AppUsageSession.Create(
                            activeSession.AppId,
                            new TimeRange(activeSession.StartTime, now)
                        )
                    );

                foreach (var session in data.AppUsageSessions)
                {
                    if (now <= session.EndTime)
                    {
                        skippedAppUsageSessions++;
                        continue;
                    }

                    var hasOverlap = existingSessions.Any(s =>
                        s.UsagePeriod.Overlaps(new TimeRange(session.StartTime, session.EndTime))
                    );
                    if (hasOverlap)
                    {
                        skippedAppUsageSessions++;
                        continue;
                    }

                    if (!existingApps.TryGetValue(session.AppProcessName, out var app))
                    {
                        app = App.Import(
                            session.AppProcessName,
                            App.GenerateColor(),
                            session.AppProcessName,
                            true,
                            AppCategory.UncategorizedId,
                            null
                        );
                        context.Apps.Add(app);
                        existingApps[session.AppProcessName] = app;
                    }

                    var usageSession = AppUsageSession.Import(
                        app.Id,
                        new TimeRange(session.StartTime, session.EndTime)
                    );
                    existingSessions.Add(usageSession);
                    context.AppUsageSessions.Add(usageSession);
                    importedAppUsageSessions++;
                }
            }
        }

        {
            // WebsiteCategory
            var existingCategories = await context.WebsiteCategories.ToDictionaryAsync(
                c => c.Name,
                cancellationToken
            );

            foreach (var categoryData in data.WebsiteCategories)
            {
                if (!existingCategories.TryGetValue(categoryData.Name, out var category))
                {
                    var iconPath = await SaveIconAsync(
                        categoryData.Icon,
                        categoryData.Name,
                        websiteCategoryIconDirectory,
                        cancellationToken
                    );
                    category = WebsiteCategory.Import(
                        categoryData.Name,
                        categoryData.Color,
                        iconPath
                    );
                    context.WebsiteCategories.Add(category);
                    existingCategories[categoryData.Name] = category;
                    newWebsiteCategories++;
                }
            }

            // Websites
            var existingWebsites = await context.Websites.ToDictionaryAsync(
                a => a.Host,
                cancellationToken
            );

            foreach (var websiteData in data.Websites)
            {
                if (!existingWebsites.TryGetValue(websiteData.Host, out var website))
                {
                    var iconPath = await SaveIconAsync(
                        websiteData.Icon,
                        websiteData.Name,
                        websiteIconDirectory,
                        cancellationToken
                    );
                    existingCategories.TryGetValue(
                        websiteData.WebsiteCategoryName,
                        out var matchedCategory
                    );
                    if (matchedCategory is null)
                    {
                        matchedCategory = WebsiteCategory.Import(
                            websiteData.WebsiteCategoryName,
                            Website.GenerateColor(),
                            null
                        );
                        context.WebsiteCategories.Add(matchedCategory);
                        existingCategories[websiteData.WebsiteCategoryName] = matchedCategory;
                        newWebsiteCategories++;
                    }
                    website = Website.Import(
                        websiteData.Name,
                        websiteData.Color,
                        websiteData.Host,
                        websiteData.AllowMetadataAutoRefresh,
                        matchedCategory.Id,
                        iconPath
                    );

                    context.Websites.Add(website);
                    existingWebsites[websiteData.Host] = website;
                    newWebsites++;
                }
            }

            if (data.WebsiteUsageSessions.Length != 0)
            {
                // WebsiteUsageSessions
                var minStart = data.WebsiteUsageSessions.Min(s => s.StartTime);
                var maxEnd = data.WebsiteUsageSessions.Max(s => s.EndTime);

                var existingSessions = await context
                    .WebsiteUsageSessions.Where(s => minStart < s.EndTime && s.StartTime < maxEnd)
                    .ToListAsync(cancellationToken);

                var activeSession = activeWebsiteUsageSessionStore.Current;
                if (
                    activeSession is not null
                    && activeSession.StartTime < maxEnd
                    && minStart < activeSession.LastActiveAt
                )
                    existingSessions.Add(
                        WebsiteUsageSession.Create(
                            activeSession.WebsiteId,
                            new TimeRange(activeSession.StartTime, activeSession.LastActiveAt)
                        )
                    );

                foreach (var session in data.WebsiteUsageSessions)
                {
                    if (now <= session.EndTime)
                    {
                        skippedWebsiteUsageSessions++;
                        continue;
                    }

                    var hasOverlap = existingSessions.Any(s =>
                        s.UsagePeriod.Overlaps(new TimeRange(session.StartTime, session.EndTime))
                    );
                    if (hasOverlap)
                    {
                        skippedWebsiteUsageSessions++;
                        continue;
                    }

                    if (!existingWebsites.TryGetValue(session.WebsiteHost, out var website))
                    {
                        website = Website.Import(
                            session.WebsiteHost,
                            Website.GenerateColor(),
                            session.WebsiteHost,
                            true,
                            WebsiteCategory.UncategorizedId,
                            null
                        );
                        context.Websites.Add(website);
                        existingWebsites[session.WebsiteHost] = website;
                    }

                    var usageSession = WebsiteUsageSession.Import(
                        website.Id,
                        new TimeRange(session.StartTime, session.EndTime)
                    );
                    existingSessions.Add(usageSession);
                    context.WebsiteUsageSessions.Add(usageSession);
                    importedWebsiteUsageSessions++;
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return new ImportDataResponse(
            newApps,
            newAppCategories,
            newWebsites,
            newWebsiteCategories,
            importedAppUsageSessions,
            skippedAppUsageSessions,
            importedWebsiteUsageSessions,
            skippedWebsiteUsageSessions
        );
    }

    private static async Task<string?> SaveIconAsync(
        ImportDataContracts.Icon? icon,
        string iconName,
        string iconDirectory,
        CancellationToken cancellationToken
    )
    {
        if (icon is null || icon.Data is null || icon.Data.Length == 0)
            return null;

        var safeIconName = Path.GetFileName(iconName);
        try
        {
            if (!Directory.Exists(iconDirectory))
                Directory.CreateDirectory(iconDirectory);
            var filePath = Path.Combine(iconDirectory, $"{safeIconName}{icon.Extension}");
            await File.WriteAllBytesAsync(filePath, icon.Data, cancellationToken);
            return filePath;
        }
        catch
        {
            return null;
        }
    }
}
