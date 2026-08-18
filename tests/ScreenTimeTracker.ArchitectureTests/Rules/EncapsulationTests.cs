using System.Reflection;
using System.Runtime.CompilerServices;
using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using ScreenTimeTracker.ArchitectureTests.Fixtures;
using Shouldly;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class EncapsulationTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;

    // 由于使用的是 Mediator 库，它的源生成器要求访问到 Handler，所以这条规则不启用
    /// <summary>
    /// 规则：垂直切片规范：Features 内部的 Handler 应该被封装（internal），防止被外部切片强引用硬调用。
    /// </summary>
    // [Fact]
    // public void SliceHandlers_ShouldBeInternal_ToPreventDirectCrossSliceInvocation() =>
    //     Classes()
    //         .That()
    //         .HaveNameEndingWith("Handler")
    //         .And()
    //         .ResideInNamespaceMatching(@".*\.Features\..*")
    //         .Should()
    //         .NotBePublic()
    //         .Check(_architecture);

    /// <summary>
    /// 规则：领域实体（Entity）必须强封装，不得暴露公共的属性修改器（set / init）。
    /// 状态修改必须通过显式的领域方法完成。
    /// </summary>
    [Fact(DisplayName = "Domain Entities should not expose public setters or initters")]
    public void Entities_ShouldNot_ExposePublicSettersOrInitters() =>
        MethodMembers()
            .That()
            .HaveNameStartingWith("set_")
            .And()
            .AreDeclaredIn(Classes().That().AreAssignableTo(typeof(BuildingBlocks.Domain.Entity)))
            .Should()
            .NotBePublic()
            .Check(_architecture);

    /// <summary>
    /// 规则：领域值对象（Value Object）必须具备不可变性。
    /// 允许 public init（用于对象创建/初始化），但绝对禁止常规的 public set（防止实例化后被修改）。
    /// </summary>
    [Fact(
        DisplayName = "Value Objects should not expose public regular setters (init setters are allowed)"
    )]
    public void ValueObjects_ShouldNot_ExposePublicRegularSetters()
    {
        var valueObjectTypes = Classes()
            .That()
            .AreAssignableTo(typeof(BuildingBlocks.Domain.IValueObject))
            .GetObjects(_architecture)
            .Select(c =>
                AppDomain
                    .CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName == c.FullName)
            )
            .Where(t => t != null && !t.IsInterface && !t.IsAbstract)
            .Cast<Type>();

        foreach (var type in valueObjectTypes)
        {
            var invalidProperties = type.GetProperties(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly
                )
                .Where(p =>
                    p.SetMethod is { IsPublic: true } setMethod
                    && !setMethod
                        .ReturnParameter.GetRequiredCustomModifiers()
                        .Contains(typeof(IsExternalInit))
                )
                .Select(p => p.Name)
                .ToList();

            invalidProperties.ShouldBeEmpty(
                $"Value Object '{type.Name}' must be immutable and cannot expose public regular setters. Violation properties: {string.Join(", ", invalidProperties)}"
            );
        }
    }
}
