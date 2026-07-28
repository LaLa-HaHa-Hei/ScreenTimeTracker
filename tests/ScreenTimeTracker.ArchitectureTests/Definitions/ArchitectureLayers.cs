using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Definitions;

public static class ArchitectureLayers
{
    public static readonly IObjectProvider<IType> Domain = Types()
        .That()
        .ResideInNamespaceMatching(@".*\.Domain($|\..*)")
        .As("Domain Layer");

    public static readonly IObjectProvider<IType> Infrastructure = Types()
        .That()
        .ResideInNamespaceMatching(@".*\.Infrastructure($|\..*)")
        .As("Infrastructure Layer");

    public static readonly IObjectProvider<IType> Features = Types()
        .That()
        .ResideInNamespaceMatching(@".*\.Features($|\..*)")
        .As("Features Layer");
}
