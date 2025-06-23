using Microsoft.CodeAnalysis;

namespace Observator.Generator.Diagnostics
{
    internal static class DiagnosticDescriptors
    {
        // Example diagnostic descriptor for OBS001
        public static readonly DiagnosticDescriptor OBS001_PartialClass = new DiagnosticDescriptor(
            id: "OBS001",
            title: "Method with [ObservatorTrace] must be in a partial class",
            messageFormat: "Method '{0}' is annotated with [ObservatorTrace] but is not in a partial class. No interception will occur until the class is made partial.",
            category: "Observator.Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        // Add more descriptors here as needed (OBS002, OBS003, ...)
    }
}
