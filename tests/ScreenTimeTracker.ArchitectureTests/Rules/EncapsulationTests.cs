using ArchUnitNET.Domain;
using ScreenTimeTracker.ArchitectureTests.Fixtures;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class EncapsulationTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;

    /* 由于使用的是 Mediator 库，它的源生成器要求访问到 Handler，所以这条规则不启用
        [Fact]
        // 垂直切片规范：Features 内部的 Handler 应该被封装（internal），防止被外部切片强引用硬调用
        public void SliceHandlers_ShouldBeInternal_ToPreventDirectCrossSliceInvocation() =>
            Classes()
                .That()
                .HaveNameEndingWith("Handler")
                .And()
                .ResideInNamespaceMatching(@".*\.Features\..*")
                .Should()
                .NotBePublic()
                .Check(_architecture);
    */
}
