using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator
{
    public class MethodToInterceptInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public MethodDeclarationSyntax MethodDeclaration { get; }
        public Diagnostic? Diagnostic { get; }

        public MethodToInterceptInfo(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodDeclaration, Diagnostic? diagnostic)
        {
            MethodSymbol = methodSymbol;
            MethodDeclaration = methodDeclaration;
            Diagnostic = diagnostic;
        }
    }
}