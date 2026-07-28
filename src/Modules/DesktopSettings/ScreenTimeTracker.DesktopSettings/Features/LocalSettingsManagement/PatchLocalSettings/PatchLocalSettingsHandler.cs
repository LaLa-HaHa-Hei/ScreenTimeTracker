using Mediator;
using Microsoft.EntityFrameworkCore;
using ScreenTimeTracker.DesktopSettings.Domain;
using ScreenTimeTracker.DesktopSettings.Infrastructure.Persistence;

namespace ScreenTimeTracker.DesktopSettings.Features.LocalSettingsManagement.PatchLocalSettings;

public class PatchLocalSettingsHandler(
    DesktopSettingsDbContext context,
    IStartupManager windowsStartupManager
) : IRequestHandler<PatchLocalSettingsCommand>
{
    public async ValueTask<Unit> Handle(
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
        return Unit.Value;
    }
}

public interface IStartupManager
{
    bool IsEnabled();
    void Enable();
    void Disable();
}
