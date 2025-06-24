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

            var containingType = methodSymbol.ContainingType;
            var loggerField = containingType.GetMembers()
                .OfType<IFieldSymbol>()
                .FirstOrDefault(f =>
                    (f.Name == ObservatorConstants.LoggerFieldName1 || f.Name == ObservatorConstants.LoggerFieldName2 ||
                     f.Name == ObservatorConstants.LoggerFieldName3 || f.Name == ObservatorConstants.LoggerFieldName4) &&
                    (f.Type.Name == ObservatorConstants.LoggerTypeName || f.Type.ToDisplayString().StartsWith(ObservatorConstants.LoggerTypePrefix)));

            return new MethodToInterceptInfo(methodSymbol, methodDecl, loggerField, null);
        }
    }
}