using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator;

public record InvocationCallSiteInfo(
    InvocationExpressionSyntax Invocation,
    IMethodSymbol TargetMethod,
    InterceptableLocation Location
);