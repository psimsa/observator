using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Observator.Generator.Diagnostics;
using System.Diagnostics;

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
                        if (methodSymbol == null) return (null, null, null, (Diagnostic)null);
                        var traceAttr = methodSymbol.GetAttributes().FirstOrDefault(attr =>
                            attr.AttributeClass?.ToDisplayString() == "Observator.Generated.ObservatorTraceAttribute" ||
                            attr.AttributeClass?.Name == "ObservatorTraceAttribute" ||
                            attr.AttributeClass?.Name == "ObservatorTrace");
                        if (traceAttr == null) return (null, null, null, (Diagnostic)null);
                        if (methodSymbol.IsAbstract) return (null, null, null, (Diagnostic)null);
                        // Logger detection
                        var containingType = methodSymbol.ContainingType;
                        var loggerField = containingType.GetMembers()
                            .OfType<IFieldSymbol>()
                            .FirstOrDefault(f =>
                                (f.Name == "_logger" || f.Name == "logger" || f.Name == "_log" || f.Name == "log") &&
                                (f.Type.Name == "ILogger" || f.Type.ToDisplayString().StartsWith("Microsoft.Extensions.Logging.ILogger")));
                        return (methodSymbol, methodDecl, loggerField, (Diagnostic)null);
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
                    var loggerField = entry.Item3 as IFieldSymbol;
                    var diagnostic = entry.Item4;
                    if (diagnostic != null)
                        spc.ReportDiagnostic(diagnostic);
                }
                var validMethods = attributedMethodsArr.Where(x => x.Item4 == null && x.Item1 != null && x.Item2 != null).ToList();
                // Update callSiteInfos to include InterceptableLocation
                var callSiteInfos = new List<(IMethodSymbol method, MethodDeclarationSyntax syntax, InvocationExpressionSyntax invocation, InterceptableLocation location, IFieldSymbol loggerField)>();
                foreach (var callEntry in callSitesArr)
                {
                    var invocation = (InvocationExpressionSyntax)callEntry.Item1;
                    var targetMethod = (IMethodSymbol)callEntry.Item2;
                    var location = (InterceptableLocation)callEntry.Item3;
                    foreach (var validEntry in validMethods)
                    {
                        var methodSymbol = validEntry.Item1;
                        var methodDecl = validEntry.Item2;
                        var loggerField = validEntry.Item3 as IFieldSymbol;
                        if (SymbolEqualityComparer.Default.Equals(targetMethod.OriginalDefinition, methodSymbol.OriginalDefinition) &&
                            SymbolEqualityComparer.Default.Equals(targetMethod.ContainingType, methodSymbol.ContainingType))
                        {
                            callSiteInfos.Add((methodSymbol, methodDecl, invocation, location, loggerField));
                            break;
                        }
                    }
                }
                // Group call sites by namespace, then by method signature
                var interceptorsByNamespace = new Dictionary<string, Dictionary<string, List<(IMethodSymbol method, InterceptableLocation location, IFieldSymbol loggerField)>>>();
                foreach (var call in callSiteInfos)
                {
                    var method = call.method;
                    var location = call.location;
                    var loggerField = call.loggerField;
                    var ns = (call.invocation.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString())
                        ?? method.ContainingType.ContainingNamespace?.ToDisplayString() ?? "";
                    var methodSig = method.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    if (!interceptorsByNamespace.TryGetValue(ns, out var methodDict))
                    {
                        methodDict = new Dictionary<string, List<(IMethodSymbol, InterceptableLocation, IFieldSymbol)>>();
                        interceptorsByNamespace[ns] = methodDict;
                    }
                    if (!methodDict.TryGetValue(methodSig, out var callList))
                    {
                        callList = new List<(IMethodSymbol, InterceptableLocation, IFieldSymbol)>();
                        methodDict[methodSig] = callList;
                    }
                    callList.Add((method, location, loggerField));
                }

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("// <auto-generated />");
                bool anyLogger = interceptorsByNamespace.Values.SelectMany(x => x.Values).SelectMany(x => x).Any(x => x.loggerField != null);
                if (anyLogger)
                {
                    sb.AppendLine("using Microsoft.Extensions.Logging;");
                }
                foreach (var nsGroup in interceptorsByNamespace)
                {
                    var ns = nsGroup.Key;
                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.AppendLine($"namespace {ns}");
                        sb.AppendLine("{");
                    }
                    sb.AppendLine("file static class Interceptor");
                    sb.AppendLine("{");
                    foreach (var methodGroup in nsGroup.Value)
                    {
                        var callList = methodGroup.Value;
                        var method = callList[0].method;
                        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var methodName = method.Name;
                        var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {p.Name}"));
                        var args = string.Join(", ", method.Parameters.Select(p => p.Name));
                        var isAsync = method.ReturnType.Name == "Task" || method.ReturnType.Name == "ValueTask";
                        var asyncModifier = isAsync ? "async " : "";
                        // Emit all InterceptsLocation attributes for this method
                        foreach (var call in callList)
                        {
                            sb.AppendLine($"    [System.Runtime.CompilerServices.InterceptsLocationAttribute({call.location.Version}, \"{call.location.Data}\")]");
                        }
                        // Extension method signature
                        sb.AppendLine($"    public static {asyncModifier}{returnType} Intercepts{methodName}(this {method.ContainingType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} @source{(parameters.Length > 0 ? ", " + parameters : "")})");
                        sb.AppendLine(GenerateInterceptorBody_Extension(methodName, args, isAsync, callList[0].loggerField));
                    }
                    sb.AppendLine("}");
                    if (!string.IsNullOrEmpty(ns))
                    {
                        sb.AppendLine("}");
                    }
                }
                // Attribute definition
                sb.AppendLine(@"
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(int version, string data) : Attribute { }
}
");
                spc.AddSource("ObservatorInterceptors.g.cs", SourceText.From(sb.ToString(), System.Text.Encoding.UTF8));
            });
        }

        // Helper for interceptor body
        // Generates the body for extension interceptor methods
        private static string GenerateInterceptorBody_Extension(string methodName, string args, bool isAsync, IFieldSymbol loggerField)
        {
            var awaitPrefix = isAsync ? "await " : "";
            var returnPrefix = isAsync || !string.IsNullOrEmpty(args) ? "return " : "";
            var result = new System.Text.StringBuilder();
            result.AppendLine("    {");
            result.AppendLine($"        using var activity = Observator.Generated.ObservatorInfrastructure.ActivitySource.StartActivity($\"Intercepted.{{@source.GetType().Name}}.{methodName}\");");
            result.AppendLine("        try");
            result.AppendLine("        {");
            result.AppendLine($"            {returnPrefix}{awaitPrefix}@source.{methodName}({args});");
            result.AppendLine("        }");
            result.AppendLine("        catch (Exception exception)");
            result.AppendLine("        {");
            result.AppendLine("            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error, exception.Message);");
            result.AppendLine("            throw;");
            result.AppendLine("        }");
            result.AppendLine("        finally");
            result.AppendLine("        {");
            result.AppendLine("            activity?.SetStatus(activity.Status == System.Diagnostics.ActivityStatusCode.Unset ? System.Diagnostics.ActivityStatusCode.Ok : activity.Status, activity.StatusDescription)?.Dispose();");
            result.AppendLine("        }");
            result.AppendLine("    }");
            return result.ToString();
        }
    }
}
