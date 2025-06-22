using Microsoft.CodeAnalysis;

namespace Observator.Generator.Diagnostics
{
    internal static class DiagnosticReporter
    {
        public static void ReportPartialClass(DiagnosticReporterContext context, string methodName, Location location)
        {
            var diagnostic = Diagnostic.Create(
                DiagnosticDescriptors.OBS001_PartialClass,
                location,
                methodName);
            context.ReportDiagnostic(diagnostic);
        }

        // Add more reporting helpers for other diagnostics as needed
    }

    // Context wrapper for easier testing and future extensibility
    internal readonly struct DiagnosticReporterContext
    {
        private readonly GeneratorExecutionContext _generatorContext;

        public DiagnosticReporterContext(GeneratorExecutionContext context)
        {
            _generatorContext = context;
        }

        public void ReportDiagnostic(Diagnostic diagnostic)
        {
            _generatorContext.ReportDiagnostic(diagnostic);
        }
    }
}
