using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Observator.Generator.Diagnostics; // Add this line

namespace Observator.Generator;

public static class MethodAnalyzer
{
    public static MethodToInterceptInfo? AnalyzeMethodDeclaration(SyntaxNode node, GeneratorSyntaxContext ctx, CancellationToken ct)
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
        // Removed interfaceTraceAttr check

        // Removed interfaceTraceAttr check

        // If ObservatorTraceAttribute is not present, skip
        if (traceAttr == null)
            return null;

        // If method is abstract or from interface, only allow if ObservatorTraceAttribute is present
        if (methodSymbol.IsAbstract || methodSymbol.ContainingType?.TypeKind == TypeKind.Interface)
        {
            return new MethodToInterceptInfo(methodSymbol, methodDecl, null, isInterfaceMethod: true);
        }

        // If ObservatorTraceAttribute is present (and not interface method)
        return new MethodToInterceptInfo(methodSymbol, methodDecl, null, isInterfaceMethod: false);
    }

    /// <summary>
    /// Analyze an interface symbol for ObservatorTraceAttribute and return all public methods to intercept.
    /// </summary>
    public static IEnumerable<MethodToInterceptInfo> AnalyzeTypeDeclaration(INamedTypeSymbol typeSymbol)
    {
        var typeAttr = typeSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
            attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceAttributeName ||
            attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceShortName);

        if (typeAttr == null)
            yield break;

        foreach (var member in typeSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            // Only public, non-static, non-constructor methods
            if (member.DeclaredAccessibility == Accessibility.Public &&
                !member.IsStatic &&
                member.MethodKind == MethodKind.Ordinary)
            {
                // If method itself has ObservatorTraceAttribute, prefer method-level settings
                var methodAttr = member.GetAttributes().FirstOrDefault(attr =>
                    attr.AttributeClass?.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
                    attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                    attr.AttributeClass?.Name == ObservatorConstants.ObservatorTraceShortName);

                yield return new MethodToInterceptInfo(member, DiagnosticReporter.CreateStaticMethodDiagnostic(member.Name, member.Locations[0]), isInterfaceMethod: true);
            }
        }
    }
}