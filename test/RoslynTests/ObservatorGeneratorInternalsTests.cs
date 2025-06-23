// Covers internal classes from Observator.Generator
using Xunit;

namespace TestLib;

public class ObservatorGeneratorInternalsTests
{
    [Fact]
    public void DiagnosticDescriptors_Type_IsAccessible()
    {
        var type = Type.GetType("Observator.Generator.Diagnostics.DiagnosticDescriptors, Observator.Generator");
        Assert.NotNull(type);
        Assert.True(type.IsAbstract && type.IsSealed); // static class
    }

    [Fact]
    public void DiagnosticReporter_Type_IsAccessible()
    {
        var type = Type.GetType("Observator.Generator.Diagnostics.DiagnosticReporter, Observator.Generator");
        Assert.NotNull(type);
        Assert.True(type.IsAbstract && type.IsSealed); // static class
    }
}