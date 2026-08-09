using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.DeleteWebsiteCategory;

public record DeleteWebsiteCategoryCommand(Guid WebsiteCategoryId) : IRequest<ErrorOr<Deleted>>;
