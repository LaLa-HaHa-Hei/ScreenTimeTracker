using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using ScreenTimeTracker.ArchitectureTests.Fixtures;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ScreenTimeTracker.ArchitectureTests.Rules;

public class CodingStandardTests(ArchitectureFixture fixture)
{
    private readonly Architecture _architecture = fixture.Architecture;
}
