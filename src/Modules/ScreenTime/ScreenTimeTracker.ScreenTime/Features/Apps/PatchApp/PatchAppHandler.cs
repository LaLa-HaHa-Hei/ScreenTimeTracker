using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Apps;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.PatchApp;

public class PatchAppHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<PatchAppCommand, ErrorOr<Updated>>
{
    public async ValueTask<ErrorOr<Updated>> Handle(
        PatchAppCommand request,
        CancellationToken cancellationToken
    )
    {
        App? app = await context.Apps.FindAsync([request.AppId], cancellationToken);

        if (app is null)
            return Error.NotFound(
                code: "App.NotFound",
                description: "The app with the specified ID was not found."
            );

        if (request.Name.HasValue)
        {
            var exists = await context.Apps.AnyAsync(
                x => x.Id != request.AppId && x.Name == request.Name.Value,
                cancellationToken
            );
            if (exists)
                return Error.Conflict(
                    "App.NameAlreadyExists",
                    "An app with the same name already exists."
                );
        }

        if (request.CategoryId.HasValue)
        {
            var exists = await context.AppCategories.AnyAsync(
                x => x.Id == request.CategoryId.Value,
                cancellationToken
            );
            if (!exists)
                return Error.Conflict(
                    "App.CategoryNotFound",
                    "The app category with the specified ID was not found."
                );
        }

        app.Update(
            request.Name,
            request.Color,
            request.AllowMetadataAutoRefresh,
            request.CategoryId,
            request.IconPath,
            timeProvider.GetUtcNow()
        );

        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }
}
