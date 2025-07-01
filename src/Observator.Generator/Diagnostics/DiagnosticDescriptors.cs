using Microsoft.CodeAnalysis;

namespace Observator.Generator.Diagnostics;

internal static partial class DiagnosticDescriptors
{
    // Diagnostic descriptor for OBS006
    public static readonly DiagnosticDescriptor OBS006_StaticMethod = new DiagnosticDescriptor(
        id: "OBS006",
        title: "Method with [ObservatorTrace] must not be static",
        messageFormat: "Method '{0}' is annotated with [ObservatorTrace] but is static. No interception will occur for static methods.",
        category: "Observator.Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}