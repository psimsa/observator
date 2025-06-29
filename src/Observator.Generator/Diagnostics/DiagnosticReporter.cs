using Microsoft.CodeAnalysis;

namespace Observator.Generator.Diagnostics;

internal static class DiagnosticReporter
{
    public static Diagnostic CreateStaticMethodDiagnostic(string methodName, Location location)
    {
        return Diagnostic.Create(
            DiagnosticDescriptors.OBS006_StaticMethod,
            location,
            methodName);
    }
}