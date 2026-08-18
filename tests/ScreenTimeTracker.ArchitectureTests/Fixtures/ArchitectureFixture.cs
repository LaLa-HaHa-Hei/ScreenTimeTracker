using ArchUnitNET.Domain;
using ArchUnitNET.Loader;

[assembly: AssemblyFixture(
    typeof(ScreenTimeTracker.ArchitectureTests.Fixtures.ArchitectureFixture)
)]

namespace ScreenTimeTracker.ArchitectureTests.Fixtures;

public class ArchitectureFixture
{
    public Architecture Architecture { get; }

    public ArchitectureFixture()
    {
        Architecture = new ArchLoader()
            .LoadAssemblies(
                typeof(BuildingBlocks.Domain.Entity).Assembly,
                typeof(ScreenTime.Domain.Aggregates.Apps.App).Assembly,
                typeof(DesktopSettings.Domain.LocalSettings).Assembly,
                typeof(DesktopSettings.Contracts.Enums.UIOpenMode).Assembly,
                typeof(Desktop.DesktopGroup).Assembly
            )
            .Build();
    }
}
