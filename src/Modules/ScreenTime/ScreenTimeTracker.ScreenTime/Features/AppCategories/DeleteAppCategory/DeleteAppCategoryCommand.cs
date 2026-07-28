using Mediator;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.DeleteAppCategory;

public record DeleteAppCategoryCommand(Guid AppCategoryId) : IRequest;
