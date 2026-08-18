using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.CreateAppCategory;

public class CreateAppCategoryHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<CreateAppCategoryCommand, ErrorOr<CreateAppCategoryResponse>>
{
    public async ValueTask<ErrorOr<CreateAppCategoryResponse>> Handle(
        CreateAppCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var exists = await context.AppCategories.AnyAsync(
            x => x.Name == request.Name,
            cancellationToken
        );
        if (exists)
            return Error.Conflict(
                "AppCategory.NameAlreadyExists",
                "An app category with the same name already exists."
            );

        AppCategory appCategory = AppCategory.Create(
            timeProvider.GetUtcNow(),
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
