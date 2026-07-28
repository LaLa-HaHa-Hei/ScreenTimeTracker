using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApps;

public record GetAppsQuery(
    string? Fields // 逗号分隔的字段列表
) : IRequest<List<Dictionary<string, object?>>>;
