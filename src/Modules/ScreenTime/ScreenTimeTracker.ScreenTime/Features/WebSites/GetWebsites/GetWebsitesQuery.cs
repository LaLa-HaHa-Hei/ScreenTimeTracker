using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.GetWebsites;

public record GetWebsitesQuery(
    string? Fields // 逗号分隔的字段列表
) : IRequest<List<Dictionary<string, object?>>>;
