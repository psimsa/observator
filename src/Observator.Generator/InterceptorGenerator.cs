using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Observator.Generator.Diagnostics;

namespace Observator.Generator
{
    [Generator]
    public class InterceptorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all method declarations with [ObservatorTrace]
            var attributedMethods = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is MethodDeclarationSyntax,
                    transform: (ctx, ct) =>
                    {
                        var methodDecl = (MethodDeclarationSyntax)ctx.Node;
                        var model = ctx.SemanticModel;
                        var methodSymbol = model.GetDeclaredSymbol(methodDecl, ct) as IMethodSymbol;
                        if (methodSymbol == null) return (null, null, (Diagnostic)null);
                        var traceAttr = methodSymbol.GetAttributes().FirstOrDefault(attr =>
                            attr.AttributeClass?.ToDisplayString() == "Observator.Generated.ObservatorTraceAttribute" ||
                            attr.AttributeClass?.Name == "ObservatorTraceAttribute" ||
                            attr.AttributeClass?.Name == "ObservatorTrace");
                        if (traceAttr == null) return (null, null, (Diagnostic)null);
                        if (methodSymbol.IsAbstract) return (null, null, (Diagnostic)null);
                        // OBS001: Method must be in a partial class
                        var containingType = methodSymbol.ContainingType;
                        if (!containingType.DeclaringSyntaxReferences.Any(syntaxRef =>
                            (syntaxRef.GetSyntax(ct) as TypeDeclarationSyntax)?.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)) == true))
                        {
                            // Report diagnostic via pipeline (see below)
                            return (methodSymbol, methodDecl, Diagnostic.Create(
                                DiagnosticDescriptors.OBS001_PartialClass,
                                methodDecl.Identifier.GetLocation(),
                                methodSymbol.Name));
                        }
                        return (methodSymbol, methodDecl, (Diagnostic)null);
                    })
                .Where(x => x.Item1 != null)
                .Select((x, _) => x);

            var callSites = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is InvocationExpressionSyntax,
                    transform: (ctx, ct) =>
                    {
                        var invocation = (InvocationExpressionSyntax)ctx.Node;
                        var model = ctx.SemanticModel;
                        var symbolInfo = model.GetSymbolInfo(invocation, ct);
                        var targetMethod = symbolInfo.Symbol as IMethodSymbol;
                        if (targetMethod == null) return (null, null, null);
                        // Use the new interceptors API
                        var interceptableLocation = model.GetInterceptableLocation(invocation, ct);
                        if (interceptableLocation == null)
                            return (null, null, null);
                        return (invocation, targetMethod, interceptableLocation);
                    })
                .Where(x => x.Item1 != null && x.Item2 != null && x.Item3 != null)
                .Select((x, _) => x);

            // Combine attributed methods and call sites for code generation
            var combined = attributedMethods.Collect().Combine(callSites.Collect());

            context.RegisterSourceOutput(combined, (spc, tuple) =>
            {
                var attributedMethodsArr = tuple.Left;
                var callSitesArr = tuple.Right;
                foreach (var entry in attributedMethodsArr)
                {
                    var methodSymbol = entry.Item1;
                    var methodDecl = entry.Item2;
                    var diagnostic = entry.Item3;
                    if (diagnostic != null)
                        spc.ReportDiagnostic(diagnostic);
                }
                var validMethods = attributedMethodsArr.Where(x => x.Item3 == null && x.Item1 != null && x.Item2 != null).ToList();
                // Update callSiteInfos to include InterceptableLocation
                var callSiteInfos = new List<(IMethodSymbol method, MethodDeclarationSyntax syntax, InvocationExpressionSyntax invocation, InterceptableLocation location)>();
                foreach (var callEntry in callSitesArr)
                {
                    var invocation = (InvocationExpressionSyntax)callEntry.Item1;
                    var targetMethod = (IMethodSymbol)callEntry.Item2;
                    var location = (InterceptableLocation)callEntry.Item3;
                    foreach (var validEntry in validMethods)
                    {
                        var methodSymbol = validEntry.Item1;
                        var methodDecl = validEntry.Item2;
                        if (SymbolEqualityComparer.Default.Equals(targetMethod.OriginalDefinition, methodSymbol.OriginalDefinition) &&
                            SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, methodSymbol.ContainingType))
                        {
                            callSiteInfos.Add((methodSymbol, methodDecl, invocation, location));
                            break;
                        }
                    }
                }
                // Group call sites by containing class and method signature (as strings)
                var interceptorsByClassAndMethod = new Dictionary<(string className, string methodSig), List<(IMethodSymbol method, MethodDeclarationSyntax syntax, InvocationExpressionSyntax invocation, InterceptableLocation location)>>();
                foreach (var call in callSiteInfos)
                {
                    var method = call.method;
                    var syntax = call.syntax;
                    var invocation = call.invocation;
                    var location = call.location;
                    var containingType = method.ContainingType;
                    var className = containingType.ToDisplayString();
                    var methodSig = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    var key = (className, methodSig);
                    if (!interceptorsByClassAndMethod.TryGetValue(key, out var list))
                    {
                        list = new List<(IMethodSymbol, MethodDeclarationSyntax, InvocationExpressionSyntax, InterceptableLocation)>();
                        interceptorsByClassAndMethod[key] = list;
                    }
                    list.Add((method, syntax, invocation, location));
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("// <auto-generated />");
                // Group by class for emission
                var classGroups = interceptorsByClassAndMethod.GroupBy(kvp => kvp.Key.className);
                foreach (var classGroup in classGroups)
                {
                    var className = classGroup.Key;
                    // Find the namespace from the first method in the group
                    var firstMethod = classGroup.First().Value[0].method;
                    var ns = firstMethod.ContainingType.ContainingNamespace?.ToDisplayString() ?? "";
                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.AppendLine($"namespace {ns}");
                        sb.AppendLine("{");
                    }
                    sb.AppendLine($"partial class {firstMethod.ContainingType.Name}");
                    sb.AppendLine("{");
                    foreach (var methodGroup in classGroup)
                    {
                        var callList = methodGroup.Value;
                        var method = callList[0].method;
                        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var methodName = method.Name;
                        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {p.Name}"));
                        var args = string.Join(", ", method.Parameters.Select(p => p.Name));
                        var isAsync = method.ReturnType.Name == "Task" || method.ReturnType.Name == "ValueTask";
                        var asyncModifier = isAsync ? "async " : "";
                        var awaitPrefix = isAsync ? "await " : "";
                        var isIterator = callList[0].syntax.DescendantNodes().OfType<YieldStatementSyntax>().Any();
                        // Emit a private clone of the original method (actual body)
                        var cloneName = methodName + "_Clone";
                        sb.AppendLine($"    private {asyncModifier}{returnType} {cloneName}({parameters})");
                        if (callList[0].syntax.Body != null)
                        {
                            // Block body: copy exactly
                            sb.AppendLine(callList[0].syntax.Body.ToFullString());
                        }
                        else if (callList[0].syntax.ExpressionBody != null)
                        {
                            // Expression-bodied
                            sb.AppendLine($"    {{ return {callList[0].syntax.ExpressionBody.Expression.ToFullString()}; }}");
                        }
                        else
                        {
                            sb.AppendLine("    { throw new System.NotImplementedException(); }");
                        }
                        // Emit the interceptor method with InterceptsLocation attribute(s)
                        foreach (var (_, _, _, location) in callList)
                        {
                            sb.AppendLine($"    [System.Runtime.CompilerServices.InterceptsLocation(1, \"{location.Data}\")]" );
                        }
                        var interceptorName = methodName + "_Interceptor";
                        sb.AppendLine($"    internal {asyncModifier}{returnType} {interceptorName}({parameters})");
                        sb.AppendLine(GenerateInterceptorBody(methodName, cloneName, args, isAsync));
                    }
                    sb.AppendLine("}");
                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.AppendLine("}");
                    }
                }
                spc.AddSource("ObservatorInterceptors.g.cs", SourceText.From(sb.ToString(), System.Text.Encoding.UTF8));
            });
        }

        // Helper for interceptor body
        private static string GenerateInterceptorBody(string methodName, string cloneName, string args, bool isAsync)
        {
            var awaitPrefix = isAsync ? "await " : "";
            var returnPrefix = isAsync || !string.IsNullOrEmpty(args) ? "return " : "";
            var result = new System.Text.StringBuilder();
            result.AppendLine("    {");
            result.AppendLine($"        System.Console.WriteLine(\"[Observator] {methodName} started\");");
            result.AppendLine("        try");
            result.AppendLine("        {");
            result.AppendLine($"            {returnPrefix}{awaitPrefix}this.{cloneName}({args});");
            result.AppendLine("        }");
            result.AppendLine("        catch (Exception ex)");
            result.AppendLine("        {");
            result.AppendLine($"            System.Console.WriteLine(\"[Observator] {methodName} exception: {{ex}}\");");
            result.AppendLine("            throw;");
            result.AppendLine("        }");
            result.AppendLine("        finally");
            result.AppendLine("        {");
            result.AppendLine($"            System.Console.WriteLine(\"[Observator] {methodName} ended\");");
            result.AppendLine("        }");
            result.AppendLine("    }");
            return result.ToString();
        }
    }
}
