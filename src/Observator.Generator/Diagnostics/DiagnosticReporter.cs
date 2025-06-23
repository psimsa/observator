using Microsoft.CodeAnalysis;

namespace Observator.Generator.Diagnostics
{
    internal static class DiagnosticReporter
    {
        public static Diagnostic CreatePartialClassDiagnostic(string methodName, Location location)
        {
            return Diagnostic.Create(
                DiagnosticDescriptors.OBS001_PartialClass,
                location,
                methodName);
        }

        // Add more reporting helpers for other diagnostics as needed
    }
}
