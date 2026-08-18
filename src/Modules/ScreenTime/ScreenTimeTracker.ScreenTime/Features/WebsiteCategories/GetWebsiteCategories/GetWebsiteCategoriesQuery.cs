using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategories;

public record GetWebsiteCategoriesQuery(
    string? Fields // 逗号分隔的字段列表
) : IRequest<List<Dictionary<string, object?>>>;
