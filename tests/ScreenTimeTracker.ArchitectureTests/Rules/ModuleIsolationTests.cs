using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using ScreenTimeTracker.ArchitectureTests.Definitions;
using ScreenTimeTracker.ArchitectureTests.Fixtures;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class ModuleIsolationTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;

    [Fact]
    // BuildingBlocks 不应依赖业务模块或 Host
    public void BuildingBlocks_ShouldNotDependOn_AnyBusinessModulesOrHost() =>
        Types()
            .That()
            .Are(ArchitectureModules.BuildingBlocks)
            .Should()
            .NotDependOnAny(ArchitectureModules.ScreenTime)
            .AndShould()
            .NotDependOnAny(ArchitectureModules.Desktop)
            .WithoutRequiringPositiveResults()
            .Check(_architecture);

    [Fact] // ScreenTime 不应依赖 DesktopSettings
    public void ScreenTime_ShouldNotDependOn_DesktopSettings() =>
        Types()
            .That()
            .Are(ArchitectureModules.ScreenTime)
            .Should()
            .NotDependOnAny(ArchitectureModules.DesktopSettings)
            .WithoutRequiringPositiveResults()
            .Check(_architecture);

    [Fact] // DesktopSettings 不应依赖 ScreenTime
    public void DesktopSettings_ShouldNotDependOn_ScreenTime() =>
        Types()
            .That()
            .Are(ArchitectureModules.DesktopSettings)
            .Should()
            .NotDependOnAny(ArchitectureModules.ScreenTime)
            .WithoutRequiringPositiveResults()
            .Check(_architecture);

    [Fact]
    // 任何业务模块都不应依赖 Host
    public void AnyBusinessModule_ShouldNotDependOn_Host() =>
        Types()
            .That()
            .Are(ArchitectureModules.ScreenTime)
            .Or()
            .Are(ArchitectureModules.DesktopSettings)
            .Should()
            .NotDependOnAny(ArchitectureModules.Desktop)
            .WithoutRequiringPositiveResults()
            .Check(_architecture);
}
