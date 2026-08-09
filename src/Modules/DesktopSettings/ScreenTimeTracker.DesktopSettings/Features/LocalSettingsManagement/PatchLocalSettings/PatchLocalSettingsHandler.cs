using ErrorOr;
using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.DesktopSettings.Domain;
using ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public class PatchLocalSettingsHandler(
    DesktopSettingsDbContext context,
    IStartupManager windowsStartupManager
) : IRequestHandler<PatchLocalSettingsCommand, ErrorOr<Updated>>
{
    public async ValueTask<ErrorOr<Updated>> Handle(
        PatchLocalSettingsCommand request,
        CancellationToken cancellationToken
    )
    {
        LocalSettings localSettings = await context.LocalSettings.SingleAsync(cancellationToken);

        localSettings.Update(
            request.DefaultUIOpenMode,
            request.IsAutoStartEnabled,
            request.IsSilentStartEnabled,
            request.Language
        );

        if (request.IsAutoStartEnabled.HasValue)
        {
            if (request.IsAutoStartEnabled.Value)
                windowsStartupManager.Enable();
            else
                windowsStartupManager.Disable();
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Updated;
    }
}

public interface IStartupManager
{
    bool IsEnabled();
    void Enable();
    void Disable();
}
