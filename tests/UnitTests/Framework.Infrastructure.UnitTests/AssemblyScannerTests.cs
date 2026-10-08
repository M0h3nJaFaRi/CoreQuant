using Framework.Infrastructure.Reflection;

namespace Framework.Infrastructure.UnitTests;

public sealed class AssemblyScannerTests
{
    [Fact]
    public void GetTypesAssignableTo_Returns_Concrete_Implementations()
    {
        var types = AssemblyScanner.GetTypesAssignableTo<ISampleMarker>(typeof(AssemblyScannerTests).Assembly);

        Assert.Contains(typeof(SampleImplementation), types);
        Assert.DoesNotContain(typeof(AbstractSample), types);
        Assert.DoesNotContain(typeof(ISampleMarker), types);
    }

    [Fact]
    public void GetTypesAssignableTo_Throws_When_Assembly_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => AssemblyScanner.GetTypesAssignableTo<ISampleMarker>(null!));
    }

    private interface ISampleMarker;

    private abstract class AbstractSample : ISampleMarker;

    private sealed class SampleImplementation : ISampleMarker;
}
