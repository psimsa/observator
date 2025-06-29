using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator;

public record InterceptorCandidateInfo(
    IMethodSymbol MethodSymbol,
    MethodDeclarationSyntax? MethodDeclaration,
    InvocationExpressionSyntax Invocation,
    InterceptableLocation Location
);