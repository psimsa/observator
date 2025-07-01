using Microsoft.CodeAnalysis;

namespace Observator.Generator.Diagnostics;

internal static partial class DiagnosticDescriptors
{
    // Error for class+method level attribute conflict
    public static readonly DiagnosticDescriptor OBS007_ClassAndMethodLevelAttributeConflict = new DiagnosticDescriptor(
        id: "OBS007",
        title: "Cannot use [ObservatorTrace] on both class and its methods",
        messageFormat: "Class '{0}' is decorated with [ObservatorTrace], so method '{1}' cannot also be decorated. Use either class-level or method-level, not both.",
        category: "Observator.Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
