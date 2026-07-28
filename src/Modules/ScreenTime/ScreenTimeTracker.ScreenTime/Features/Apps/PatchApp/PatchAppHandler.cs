using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public class PatchAppHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<PatchAppCommand>
{
    public async ValueTask<Unit> Handle(
        PatchAppCommand request,
        CancellationToken cancellationToken
    )
    {
        App? app = await context.Apps.FindAsync([request.Id], cancellationToken);
        if (app is null)
            return Unit.Value;

        if (request.AppCategoryId.HasValue)
        {
            bool categoryExists = await context.AppCategories.AnyAsync(
                x => x.Id == request.AppCategoryId.Value,
                cancellationToken
            );
            if (!categoryExists)
                return Unit.Value;
        }

        app.Update(
            request.Name,
            request.Color,
            request.AllowMetadataAutoUpdate,
            request.AppCategoryId,
            request.IconPath,
            timeProvider.GetLocalNow().DateTime
        );

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
