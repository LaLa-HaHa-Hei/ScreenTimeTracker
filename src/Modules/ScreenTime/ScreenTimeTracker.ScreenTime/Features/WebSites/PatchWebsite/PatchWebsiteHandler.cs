using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Domain.Websites;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Websites.PatchWebsite;

public class PatchWebsiteHandler(ScreenTimeDbContext context, TimeProvider timeProvider)
    : IRequestHandler<PatchWebsiteCommand, ErrorOr<Updated>>
{
    public async ValueTask<ErrorOr<Updated>> Handle(
        PatchWebsiteCommand request,
        CancellationToken cancellationToken
    )
    {
        Website? website = await context.Websites.FindAsync([request.WebsiteId], cancellationToken);

        if (website is null)
            return Error.NotFound(
                code: "Website.NotFound",
                description: "The website with the specified ID was not found."
            );

        if (request.CategoryId.HasValue)
        {
            var exists = await context.WebsiteCategories.AnyAsync(
                x => x.Id != request.WebsiteId && x.Name == request.Name.Value,
                cancellationToken
            );
            if (exists)
                return Error.Conflict(
                    "Website.NameAlreadyExists",
                    "An website with the same name already exists."
                );
        }

        website.Update(
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
