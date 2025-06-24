using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;

namespace Observator.Generator
{
    public static class MethodAnalyzer
    {
        public static MethodToInterceptInfo AnalyzeMethodDeclaration(SyntaxNode node, GeneratorSyntaxContext ctx, CancellationToken ct)
        {
            var methodDecl = (MethodDeclarationSyntax)node;
            var model = ctx.SemanticModel;
            var methodSymbol = model.GetDeclaredSymbol(methodDecl, ct) as IMethodSymbol;
            if (methodSymbol == null) return null;

            var traceAttr = methodSymbol.GetAttributes().FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceShortName);

            if (traceAttr == null) return null;
            if (methodSymbol.IsAbstract) return null;

            return new MethodToInterceptInfo(methodSymbol, methodDecl, null);
        }
    }
}