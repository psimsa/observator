using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;

namespace Observator.Generator;

public static class CallSiteAnalyzer
{
    public static InvocationCallSiteInfo? AnalyzeInvocationExpression(SyntaxNode node, SemanticModel model, CancellationToken ct)
    {
        var invocation = (InvocationExpressionSyntax)node;
        var symbolInfo = model.GetSymbolInfo(invocation, ct);
        if (symbolInfo.Symbol is not IMethodSymbol targetMethod) return null;

        var interceptableLocation = model.GetInterceptableLocation(invocation, ct);
        return interceptableLocation == null ? null : new InvocationCallSiteInfo(invocation, targetMethod, interceptableLocation);
    }
}