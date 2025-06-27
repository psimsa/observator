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
        var targetMethod = symbolInfo.Symbol as IMethodSymbol;
        if (targetMethod == null) return null;

        var interceptableLocation = model.GetInterceptableLocation(invocation, ct);
        if (interceptableLocation == null) return null;

        return new InvocationCallSiteInfo(invocation, targetMethod, interceptableLocation);
    }
}