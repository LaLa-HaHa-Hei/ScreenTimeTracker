using System.Linq.Expressions;
using System.Text.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.WebsiteCategories;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategories;

public class GetWebsiteCategoriesHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetWebsiteCategoriesQuery, List<Dictionary<string, object?>>>
{
    // 定义响应字段与实体属性的依赖关系
    private static readonly Dictionary<string, string[]> _fieldDependencies = new(
        StringComparer.OrdinalIgnoreCase
    )
    {
        { nameof(GetWebsiteCategoriesResponseItem.Id), [nameof(WebsiteCategory.Id)] },
        { nameof(GetWebsiteCategoriesResponseItem.Name), [nameof(WebsiteCategory.Name)] },
        { nameof(GetWebsiteCategoriesResponseItem.Color), [nameof(WebsiteCategory.Color)] },
        { nameof(GetWebsiteCategoriesResponseItem.IconPath), [nameof(WebsiteCategory.IconPath)] },
        {
            nameof(GetWebsiteCategoriesResponseItem.IconPathLastUpdatedAt),
            [nameof(WebsiteCategory.IconPathLastUpdatedAt)]
        },
        { nameof(GetWebsiteCategoriesResponseItem.IsSystem), [nameof(WebsiteCategory.IsSystem)] },
    };

    // 各字段的计算方式字典
    private static readonly Dictionary<string, Func<WebsiteCategory, object?>> _fieldCalculations =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(GetWebsiteCategoriesResponseItem.Id)] = e => e.Id,
            [nameof(GetWebsiteCategoriesResponseItem.Name)] = e => e.Name,
            [nameof(GetWebsiteCategoriesResponseItem.Color)] = e => e.Color,
            [nameof(GetWebsiteCategoriesResponseItem.IconPath)] = e => e.IconPath,
            [nameof(GetWebsiteCategoriesResponseItem.IconPathLastUpdatedAt)] = e =>
                e.IconPathLastUpdatedAt,
            [nameof(GetWebsiteCategoriesResponseItem.IsSystem)] = e => e.IsSystem,
        };

    public async ValueTask<List<Dictionary<string, object?>>> Handle(
        GetWebsiteCategoriesQuery request,
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

        var query = context.WebsiteCategories.AsNoTracking().OrderBy(a => a.Id).AsQueryable();

        // 数据库投影：只查询 WebsiteCategory 的必要字段
        var projectedQuery = query.Select(BuildEntitySelector(targetFields));
        List<WebsiteCategory> dataEntities = await projectedQuery.ToListAsync(cancellationToken);

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

    private static Expression<Func<WebsiteCategory, WebsiteCategory>> BuildEntitySelector(
        IEnumerable<string> finalFields
    )
    {
        var parameter = Expression.Parameter(typeof(WebsiteCategory), "x");
        var neededEntityProps = new HashSet<string>();

        // 查找这些 Key 对应 WebsiteCategory 实体的哪些属性
        foreach (var field in finalFields)
        {
            if (_fieldDependencies.TryGetValue(field, out var deps))
                foreach (var d in deps)
                    neededEntityProps.Add(d);
        }

        var bindings = new List<MemberBinding>();
        foreach (var propName in neededEntityProps)
        {
            var property = typeof(WebsiteCategory).GetProperty(propName);
            if (property is null)
                continue;

            bindings.Add(Expression.Bind(property, Expression.Property(parameter, property)));
        }

        return Expression.Lambda<Func<WebsiteCategory, WebsiteCategory>>(
            Expression.MemberInit(Expression.New(typeof(WebsiteCategory)), bindings),
            parameter
        );
    }
}
