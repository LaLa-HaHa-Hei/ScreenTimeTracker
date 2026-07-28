using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using ScreenTimeTracker.ArchitectureTests.Definitions;
using ScreenTimeTracker.ArchitectureTests.Fixtures;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class LayeringTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;

    [Fact]
    // Domain 绝对纯净，不应依赖 Infrastructure 或 Features
    public void DomainLayer_ShouldNotDependOn_InfrastructureOrFeatures() =>
        Types()
            .That()
            .Are(ArchitectureLayers.Domain)
            .Should()
            .NotDependOnAny(ArchitectureLayers.Infrastructure)
            .AndShould()
            .NotDependOnAny(ArchitectureLayers.Features)
            .Check(_architecture);
}
