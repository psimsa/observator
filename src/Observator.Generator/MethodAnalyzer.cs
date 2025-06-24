using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

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

            var attributes = methodSymbol.GetAttributes();

            var traceAttr = attributes.FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceShortName);

            var interfaceTraceAttr = attributes.FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorInterfaceTraceAttributeFullName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorInterfaceTraceAttributeName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorInterfaceTraceAttributeShortName);

            // If neither attribute is present, skip
            if (traceAttr == null && interfaceTraceAttr == null)
                return null;

            // If method is abstract or from interface, only allow if ObservatorInterfaceTraceAttribute is present
            if (methodSymbol.IsAbstract || methodSymbol.ContainingType?.TypeKind == TypeKind.Interface)
            {
                if (interfaceTraceAttr == null)
                    return null;
                return new MethodToInterceptInfo(methodSymbol, methodDecl, null, isInterfaceMethod: true);
            }

            // If ObservatorTraceAttribute is present (and not interface method)
            if (traceAttr != null)
                return new MethodToInterceptInfo(methodSymbol, methodDecl, null, isInterfaceMethod: false);

            // If only ObservatorInterfaceTraceAttribute is present on a non-abstract method
            if (interfaceTraceAttr != null)
                return new MethodToInterceptInfo(methodSymbol, methodDecl, null, isInterfaceMethod: true);

            return null;
        }

        /// <summary>
        /// Analyze an interface symbol for ObservatorInterfaceTraceAttribute and return all public methods to intercept.
        /// </summary>
        public static IEnumerable<MethodToInterceptInfo> AnalyzeInterfaceDeclaration(INamedTypeSymbol interfaceSymbol)
        {
            var interfaceAttr = interfaceSymbol.GetAttributes().FirstOrDefault(attr =>
                attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorInterfaceTraceAttributeFullName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorInterfaceTraceAttributeName ||
                attr.AttributeClass?.Name == ObservatorConstants.ObservatorInterfaceTraceAttributeShortName);

            if (interfaceAttr == null)
                yield break;

            foreach (var member in interfaceSymbol.GetMembers().OfType<IMethodSymbol>())
            {
                // Only public, non-static, non-constructor methods
                if (member.DeclaredAccessibility == Accessibility.Public &&
                    !member.IsStatic &&
                    member.MethodKind == MethodKind.Ordinary)
                {
                    // If method itself has ObservatorInterfaceTraceAttribute, prefer method-level settings
                    var methodAttr = member.GetAttributes().FirstOrDefault(attr =>
                        attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorInterfaceTraceAttributeFullName ||
                        attr.AttributeClass?.Name == ObservatorConstants.ObservatorInterfaceTraceAttributeName ||
                        attr.AttributeClass?.Name == ObservatorConstants.ObservatorInterfaceTraceAttributeShortName);

                    yield return new MethodToInterceptInfo(member, diagnostic: null, isInterfaceMethod: true);
                }
            }
        }
    }
}