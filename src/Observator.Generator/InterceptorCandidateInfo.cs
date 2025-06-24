using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator
{
    public class InterceptorCandidateInfo
    {
        public IMethodSymbol MethodSymbol { get; }
        public MethodDeclarationSyntax MethodDeclaration { get; }
        public InvocationExpressionSyntax Invocation { get; }
        public InterceptableLocation Location { get; }

        public InterceptorCandidateInfo(IMethodSymbol methodSymbol, MethodDeclarationSyntax methodDeclaration, InvocationExpressionSyntax invocation, InterceptableLocation location)
        {
            MethodSymbol = methodSymbol;
            MethodDeclaration = methodDeclaration;
            Invocation = invocation;
            Location = location;
        }
    }
}