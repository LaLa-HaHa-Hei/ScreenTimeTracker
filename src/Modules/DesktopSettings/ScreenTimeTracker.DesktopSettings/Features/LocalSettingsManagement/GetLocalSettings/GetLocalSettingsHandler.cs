using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.DesktopSettings.Contracts.Queries;
using ScreenTimeTracker.DesktopSettings.Domain;
using ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.GetLocalSettings;

public class GetLocalSettingsHandler(DesktopSettingsDbContext context)
    : IRequestHandler<GetLocalSettingsQuery, GetLocalSettingsResult>
{
    public async ValueTask<GetLocalSettingsResult> Handle(
        GetLocalSettingsQuery request,
        CancellationToken cancellationToken
    )
    {
        LocalSettings localSettings = await context
            .LocalSettings.AsNoTracking()
            .SingleAsync(cancellationToken);

        return new GetLocalSettingsResult(
            localSettings.DefaultUIOpenMode,
            localSettings.IsAutoStartEnabled,
            localSettings.IsSilentStartEnabled,
            localSettings.Language
        );
    }
}
