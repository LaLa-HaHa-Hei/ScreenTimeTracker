using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Domain.Exceptions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.CreateAppCategory;

public class CreateAppCategoryHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<CreateAppCategoryCommand, CreateAppCategoryResponse>
{
    public async ValueTask<CreateAppCategoryResponse> Handle(
        CreateAppCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var exists = await context.AppCategories.AnyAsync(
            x => x.Name == request.Name,
            cancellationToken
        );
        if (exists)
            throw new AppCategoryAlreadyExistsException(request.Name);

        AppCategory appCategory = AppCategory.Create(
            timeProvider.GetLocalNow().DateTime,
            request.Name,
            request.Color,
            request.IconPath
        );
        context.AppCategories.Add(appCategory);
        await context.SaveChangesAsync(cancellationToken);
        return new CreateAppCategoryResponse(
            appCategory.Id,
            appCategory.Name,
            appCategory.Color,
            appCategory.IconPath,
            appCategory.IconPathLastUpdatedAt,
            appCategory.IsSystem
        );
    }
}
