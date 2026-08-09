using ErrorOr;
using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.WebsiteCategories.CreateWebsiteCategory;

public record CreateWebsiteCategoryCommand(string Name, string Color, string? IconPath)
    : IRequest<ErrorOr<CreateWebsiteCategoryResponse>>;
