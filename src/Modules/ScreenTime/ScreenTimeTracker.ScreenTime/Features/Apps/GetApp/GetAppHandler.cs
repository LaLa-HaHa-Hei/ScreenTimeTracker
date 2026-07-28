using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.ScreenTime.Infrastructure.Persistence;

namespace ScreenTimeTracker.ScreenTime.Features.Apps.GetApp;

public class GetAppHandler(ScreenTimeDbContext context)
    : IRequestHandler<GetAppQuery, GetAppResponse?>
{
    public async ValueTask<GetAppResponse?> Handle(
        GetAppQuery request,
        CancellationToken cancellationToken
    )
    {
        var app = await context
            .Apps.AsNoTracking()
            .FirstOrDefaultAsync(App => App.Id == request.AppId, cancellationToken);
        return app is null
            ? null
            : new GetAppResponse(
                app.Id,
                app.Name,
                app.Color,
                app.ProcessName,
                app.AllowMetadataAutoUpdate,
                app.MetadataLastUpdatedAt,
                app.AppCategoryId,
                app.ExecutablePath,
                app.IconPath,
                app.IconPathLastUpdatedAt,
                app.IsSystem
            );
    }
}
