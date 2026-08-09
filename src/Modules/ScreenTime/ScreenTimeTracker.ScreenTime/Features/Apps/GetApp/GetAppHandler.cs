using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public class GetAppHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppQuery, ErrorOr<GetAppResponse>>
{
    public async ValueTask<ErrorOr<GetAppResponse>> Handle(
        GetAppQuery request,
        CancellationToken cancellationToken
    )
    {
        var app = await context
            .Apps.AsNoTracking()
            .FirstOrDefaultAsync(App => App.Id == request.AppId, cancellationToken);

        if (app is null)
            return Error.NotFound(
                code: "App.NotFound",
                description: "The app with the specified ID was not found."
            );

        return new GetAppResponse(
            app.Id,
            app.Name,
            app.Color,
            app.ProcessName,
            app.AllowMetadataAutoRefresh,
            app.MetadataLastRefreshedAt,
            app.CategoryId,
            app.ExecutablePath,
            app.IconPath,
            app.IconPathLastUpdatedAt,
            app.IsSystem
        );
    }
}
