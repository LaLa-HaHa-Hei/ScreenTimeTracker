using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Domain.Exceptions;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.AppCategories.PatchAppCategory;

public class PatchAppCategoryHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<PatchAppCategoryCommand>
{
    public async ValueTask<Unit> Handle(
        PatchAppCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        AppCategory? appCategory = await context.AppCategories.FindAsync(
            [request.AppCategoryId],
            cancellationToken
        );
        if (appCategory is null)
            return Unit.Value;

        if (request.Name.HasValue)
        {
            var exists = await context.AppCategories.AnyAsync(
                x => x.Id != request.AppCategoryId && x.Name == request.Name.Value,
                cancellationToken
            );
            if (exists)
                throw new AppCategoryAlreadyExistsException(request.Name.Value);
        }

        appCategory.Update(
            request.Name,
            request.Color,
            request.IconPath,
            timeProvider.GetLocalNow().DateTime
        );

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
