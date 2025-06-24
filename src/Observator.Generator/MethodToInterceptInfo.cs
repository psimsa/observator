using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Observator.Generator
{
    public class MethodToInterceptInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public MethodDeclarationSyntax? MethodDeclaration { get; }
        public Diagnostic? Diagnostic { get; }
        public bool IsInterfaceMethod { get; }

        public MethodToInterceptInfo(IMethodSymbol methodSymbol, MethodDeclarationSyntax? methodDeclaration, Diagnostic? diagnostic, bool isInterfaceMethod)
        {
            MethodSymbol = methodSymbol;
            MethodDeclaration = methodDeclaration;
            Diagnostic = diagnostic;
            IsInterfaceMethod = isInterfaceMethod;
        }

        // Constructor for external methods (no MethodDeclarationSyntax)
        public MethodToInterceptInfo(IMethodSymbol methodSymbol, Diagnostic? diagnostic = null, bool isInterfaceMethod = false)
        {
            MethodSymbol = methodSymbol;
            MethodDeclaration = null;
            Diagnostic = diagnostic;
            IsInterfaceMethod = isInterfaceMethod;
        }
    }
}