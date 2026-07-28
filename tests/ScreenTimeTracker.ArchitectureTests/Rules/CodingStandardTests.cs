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

    [Fact]
    // 不应直接在应用中使用 DateTime.Now 等静态属性，而应使用 TimeProvider 来获取当前时间
    public void EntireApplication_ShouldNotDirectlyUse_DateTimeStaticProperties()
    {
        // 捕获 DateTime 和 DateTimeOffset 的 get_Now / get_UtcNow Getter 方法
        var forbiddenTimeMethods = MethodMembers()
            .That()
            .AreDeclaredIn(typeof(DateTime))
            .And()
            .HaveName("get_Now")
            .Or()
            .HaveName("get_UtcNow")
            .Or()
            .AreDeclaredIn(typeof(DateTimeOffset))
            .And()
            .HaveName("get_Now")
            .Or()
            .HaveName("get_UtcNow");

        Types()
            .That()
            .ResideInNamespaceMatching(@"^ScreenTimeTracker\..*")
            // 如果确实有极个别底层 Win32 类需要例外，可以通过 AreNot 排除：
            // .And().AreNot(typeof(WindowsIdleTimeProvider))
            .Should()
            .NotCallAny(forbiddenTimeMethods)
            .Check(_architecture);
    }
}
