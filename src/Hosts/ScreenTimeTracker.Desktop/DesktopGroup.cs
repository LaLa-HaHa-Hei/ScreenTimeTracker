using FastEndpoints;

namespace ScreenTimeTracker.Desktop;

public class DesktopGroup : Group
{
    public DesktopGroup()
    {
        Configure("desktop", ep => { });
    }
}
