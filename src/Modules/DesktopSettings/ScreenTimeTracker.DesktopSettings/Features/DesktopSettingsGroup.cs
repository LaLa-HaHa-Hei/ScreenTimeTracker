using FastEndpoints;

namespace ScreenTimeTracker.DesktopSettings.Features;

public class DesktopSettingsGroup : Group
{
    public DesktopSettingsGroup()
    {
        Configure("desktop-settings", ep => { });
    }
}
