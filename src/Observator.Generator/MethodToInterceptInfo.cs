using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator
{
    public class MethodToInterceptInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public MethodDeclarationSyntax MethodDeclaration { get; }
        public IFieldSymbol? LoggerField { get; }
        public Diagnostic? Diagnostic { get; }

        public MethodToInterceptInfo(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodDeclaration, IFieldSymbol? loggerField, Diagnostic? diagnostic)
        {
            MethodSymbol = methodSymbol;
            MethodDeclaration = methodDeclaration;
            LoggerField = loggerField;
            Diagnostic = diagnostic;
        }
    }
}