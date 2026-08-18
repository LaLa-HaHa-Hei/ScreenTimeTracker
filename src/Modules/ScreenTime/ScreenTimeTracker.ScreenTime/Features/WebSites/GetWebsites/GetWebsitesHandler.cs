using System.Linq.Expressions;
using System.Text.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsites;

public class GetWebsitesHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsitesQuery, List<Dictionary<string, object?>>>
{
    // 定义响应字段与实体属性的依赖关系
    private static readonly Dictionary<string, string[]> _fieldDependencies = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        { nameof(GetWebsitesResponseItem.Id), [nameof(Website.Id)] },
        { nameof(GetWebsitesResponseItem.Name), [nameof(Website.Name)] },
        { nameof(GetWebsitesResponseItem.Color), [nameof(Website.Color)] },
        { nameof(GetWebsitesResponseItem.Host), [nameof(Website.Host)] },
        {
            nameof(GetWebsitesResponseItem.AllowMetadataAutoRefresh),
            [nameof(Website.AllowMetadataAutoRefresh)]
        },
        {
            nameof(GetWebsitesResponseItem.MetadataLastRefreshedAt),
            [nameof(Website.MetadataLastRefreshedAt)]
        },
        { nameof(GetWebsitesResponseItem.WebsiteCategoryId), [nameof(Website.WebsiteCategoryId)] },
        { nameof(GetWebsitesResponseItem.IconPath), [nameof(Website.IconPath)] },
        {
            nameof(GetWebsitesResponseItem.IconPathLastUpdatedAt),
            [nameof(Website.IconPathLastUpdatedAt)]
        },
        { nameof(GetWebsitesResponseItem.IsSystem), [nameof(Website.IsSystem)] },
    };

    // 各字段的计算方式字典
    private static readonly Dictionary<string, Func<Website, object?>> _fieldCalculations = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        [nameof(GetWebsitesResponseItem.Id)] = e => e.Id,
        [nameof(GetWebsitesResponseItem.Name)] = e => e.Name,
        [nameof(GetWebsitesResponseItem.Color)] = e => e.Color,
        [nameof(GetWebsitesResponseItem.Host)] = e => e.Host,
        [nameof(GetWebsitesResponseItem.AllowMetadataAutoRefresh)] = e =>
            e.AllowMetadataAutoRefresh,
        [nameof(GetWebsitesResponseItem.MetadataLastRefreshedAt)] = e => e.MetadataLastRefreshedAt,
        [nameof(GetWebsitesResponseItem.WebsiteCategoryId)] = e => e.WebsiteCategoryId,
        [nameof(GetWebsitesResponseItem.IconPath)] = e => e.IconPath,
        [nameof(GetWebsitesResponseItem.IconPathLastUpdatedAt)] = e => e.IconPathLastUpdatedAt,
        [nameof(GetWebsitesResponseItem.IsSystem)] = e => e.IsSystem,
    };

    public async ValueTask<List<Dictionary<string, object?>>> Handle(
        GetWebsitesQuery request,
        CancellationToken cancellationToken
    )
    {
        var allAvailableFields = _fieldDependencies.Keys.ToList();
        var requestedFields = request
            .Fields?.Split(
                ',',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            )
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var targetFields =
            (requestedFields is null || requestedFields.Count == 0)
                ? allAvailableFields
                : [.. allAvailableFields.Where(f => requestedFields.Contains(f))];

        var query = context.Websites.AsNoTracking().OrderBy(a => a.Id).AsQueryable();

        // 数据库投影：只查询 Website 的必要字段
        var projectedQuery = query.Select(BuildEntitySelector(targetFields));
        List<Website> dataEntities = await projectedQuery.ToListAsync(cancellationToken);

        // 组装结果字典
        var result = new List<Dictionary<string, object?>>(dataEntities.Count);
        foreach (var entity in dataEntities)
        {
            var dict = new Dictionary<string, object?>(
                targetFields.Count,
                StringComparer.OrdinalIgnoreCase
            );
            foreach (var field in targetFields)
            {
                var camelKey = JsonNamingPolicy.CamelCase.ConvertName(field);
                if (_fieldCalculations.TryGetValue(field, out var mwebsiteer))
                    dict[camelKey] = mwebsiteer(entity);
                else
                    dict[camelKey] = null;
            }
            result.Add(dict);
        }

        return result;
    }

    private static Expression<Func<Website, Website>> BuildEntitySelector(
        IEnumerable<string> finalFields
    )
    {
        var parameter = Expression.Parameter(typeof(Website), "x");
        var requiredEntityProps = new HashSet<string>();

        // 查找这些 Key 对应 Website 实体的哪些属性
        foreach (var field in finalFields)
        {
            if (_fieldDependencies.TryGetValue(field, out var deps))
                foreach (var d in deps)
                    requiredEntityProps.Add(d);
        }

        var bindings = new List<MemberBinding>();
        foreach (var propName in requiredEntityProps)
        {
            var property = typeof(Website).GetProperty(propName);
            if (property is null)
                continue;

            bindings.Add(Expression.Bind(property, Expression.Property(parameter, property)));
        }

        return Expression.Lambda<Func<Website, Website>>(
            Expression.MemberInit(Expression.New(typeof(Website)), bindings),
            parameter
        );
    }
}
