using FastEndpoints;

namespace ScreenTimeTracker.ScreenTime.Features;

public class ScreenTimeGroup : Group
{
    public ScreenTimeGroup()
    {
        Configure("screen-time", ep => { });
    }
}
