using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.GetWebsiteCategory;

public record GetWebsiteCategoryQuery(Guid WebsiteCategoryId)
    : IRequest<ErrorOr<GetWebsiteCategoryResponse>>;
