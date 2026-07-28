using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.GetAppCategory;

public record GetAppCategoryQuery(Guid AppCategoryId) : IRequest<GetAppCategoryResponse?>;
