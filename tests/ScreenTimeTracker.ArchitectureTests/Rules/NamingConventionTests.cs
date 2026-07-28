using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnitV3;
using ScreenTimeTracker.ArchitectureTests.Fixtures;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class NamingConventionTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;

    [Fact]
    // Domain 事件命名必须以 DomainEvent 结尾
    public void DomainEvents_ShouldHaveDomainEventSuffix() =>
        Classes()
            .That()
            .ImplementInterface(typeof(BuildingBlocks.Domain.IDomainEvent))
            .Should()
            .HaveNameEndingWith("DomainEvent")
            .Check(_architecture);
}
