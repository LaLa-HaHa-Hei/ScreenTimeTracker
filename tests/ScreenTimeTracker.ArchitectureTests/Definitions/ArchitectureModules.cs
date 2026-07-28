using ArchUnitNET.Domain;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Definitions;

public static class ArchitectureModules
{
    public static readonly IObjectProvider<IType> ScreenTime = Types()
        .That()
        .ResideInNamespace("ScreenTimeTracker.ScreenTime")
        .As("ScreenTime Module");

    public static readonly IObjectProvider<IType> DesktopSettings = Types()
        .That()
        .ResideInNamespace("ScreenTimeTracker.DesktopSettings")
        .And()
        .DoNotResideInNamespace("ScreenTimeTracker.DesktopSettings.Contracts")
        .As("DesktopSettings Module");

    public static readonly IObjectProvider<IType> DesktopSettingsContracts = Types()
        .That()
        .ResideInNamespace("ScreenTimeTracker.DesktopSettings.Contracts")
        .As("DesktopSettings Module Contracts");

    public static readonly IObjectProvider<IType> BuildingBlocks = Types()
        .That()
        .ResideInNamespace("ScreenTimeTracker.BuildingBlocks")
        .As("BuildingBlocks Shared Kernel");

    public static readonly IObjectProvider<IType> Desktop = Types()
        .That()
        .ResideInNamespace("ScreenTimeTracker.Desktop")
        .As("Desktop Host");
}
