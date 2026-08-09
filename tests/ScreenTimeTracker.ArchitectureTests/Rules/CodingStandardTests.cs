using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using ScreenTimeTracker.ArchitectureTests.Fixtures;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class CodingStandardTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;

    [Fact]
    // 领域实体类不应该暴露公共的属性变更器（如 set，init）
    public void DomainEntities_ShouldNotExposePublicPropertyMutators() =>
        MethodMembers()
            .That()
            .HaveNameStartingWith("set_")
            .And()
            .AreDeclaredIn(Classes().That().AreAssignableTo(typeof(BuildingBlocks.Domain.Entity)))
            .Should()
            .NotBePublic()
            .Check(_architecture);
}
