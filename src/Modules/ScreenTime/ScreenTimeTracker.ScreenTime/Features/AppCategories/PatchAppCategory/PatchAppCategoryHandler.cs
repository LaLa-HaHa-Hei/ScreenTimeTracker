using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Aggregates.AppCategories;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public class PatchAppCategoryHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<PatchAppCategoryCommand, ErrorOr<Updated>>
{
    public async ValueTask<ErrorOr<Updated>> Handle(
        PatchAppCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        AppCategory? appCategory = await context.AppCategories.FindAsync(
            [request.AppCategoryId],
            cancellationToken
        );

        if (appCategory is null)
            return Error.NotFound(
                code: "AppCategory.NotFound",
                description: "The app category with the specified ID was not found."
            );

        if (request.Name.HasValue)
        {
            var exists = await context.AppCategories.AnyAsync(
                x => x.Id != request.AppCategoryId && x.Name == request.Name.Value,
                cancellationToken
            );
            if (exists)
                return Error.Conflict(
                    "AppCategory.NameAlreadyExists",
                    "An app category with the same name already exists."
                );
        }

        appCategory.Update(request.Name, request.Color, request.IconPath, timeProvider.GetUtcNow());

        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }
}
