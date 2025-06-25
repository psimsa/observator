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
using System.Collections.Immutable;

namespace Observator.Generator
{
    [Generator]
    public class InterceptorGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var attributedMethods = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is MethodDeclarationSyntax,
                    transform: (ctx, ct) => MethodAnalyzer.AnalyzeMethodDeclaration(ctx.Node, ctx, ct))
                .Where(x => x != null)
                .Select((x, _) => x);

            // Interface method analysis
            var interfaceMethods = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) =>
                        node is InterfaceDeclarationSyntax ids &&
                        (
                            ids.AttributeLists.SelectMany(al => al.Attributes)
                                .Any(attr =>
                                    attr.Name.ToString().Contains("ObservatorTrace") // quick filter, refined in analyzer
                                )
                            ||
                            ids.Members.OfType<MethodDeclarationSyntax>()
                                .SelectMany(m => m.AttributeLists.SelectMany(al => al.Attributes))
                                .Any(attr =>
                                    attr.Name.ToString().Contains("ObservatorTrace")
                                )
                        ),
                    transform: (ctx, ct) =>
                        MethodAnalyzer.AnalyzeTypeDeclaration(
                            ctx.SemanticModel.GetDeclaredSymbol((InterfaceDeclarationSyntax)ctx.Node, ct) as INamedTypeSymbol
                        )
                )
                .Where(x => x != null)
                .SelectMany((x, _) => x);

            // Discover attributed methods in referenced assemblies
            var externalAttributedMethods = context.CompilationProvider.Select((compilation, _) =>
            {
                // Extracted logic for clarity and maintainability
                return GetAttributedMethodsFromReferences(compilation);
            });

            // Helper method to extract attributed methods from referenced assemblies
            static List<MethodToInterceptInfo> GetAttributedMethodsFromReferences(Compilation compilation)
            {
                var results = new List<MethodToInterceptInfo>();
                foreach (var reference in compilation.References)
                {
                    var asmSymbol = compilation.GetAssemblyOrModuleSymbol(reference) as IAssemblySymbol;
                    if (asmSymbol == null) continue;

                    foreach (var type in GetAllTypes(asmSymbol.GlobalNamespace))
                    {
                        foreach (var method in type.GetMembers().OfType<IMethodSymbol>())
                        {
                            var hasTraceAttr = method.GetAttributes().Any(attr =>
                            {
                                var attrClass = attr.AttributeClass;
                                if (attrClass == null) return false;
                                var attrName = attrClass.ToDisplayString();
                                return attrName == ObservatorConstants.ObservatorTraceAttributeFullName ||
                                       attrClass.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                                       attrClass.Name == ObservatorConstants.ObservatorTraceShortName;
                            });
                            if (hasTraceAttr)
                                results.Add(new MethodToInterceptInfo(method));
                        }
                    }
                }
                return results;
            }

            var allAttributedMethods = attributedMethods.Collect()
                .Combine(interfaceMethods.Collect())
                .Select((tuple, _) => tuple.Left.Concat(tuple.Right).ToImmutableArray())
                .Combine(externalAttributedMethods)
                .Select((tuple, _) =>
                {
                    var localAndInterface = tuple.Left;
                    var external = tuple.Right ?? new List<MethodToInterceptInfo>();
                    return localAndInterface.AddRange(external);
                });

            var callSites = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (node, _) => node is InvocationExpressionSyntax,
                    transform: (ctx, ct) => CallSiteAnalyzer.AnalyzeInvocationExpression(ctx.Node, ctx, ct))
                .Where(x => x != null)
                .Select((x, _) => x);

            var assemblyInfo = context.CompilationProvider.Select((compilation, _) =>
            {
                var assemblyName = compilation.AssemblyName ?? "Unknown";
                return assemblyName;
            });

            var combined = allAttributedMethods.Combine(callSites.Collect()).Combine(assemblyInfo);

            context.RegisterSourceOutput(combined, (spc, triple) =>
            {
                var attributedMethodsArr = triple.Left.Left;
                var callSitesArr = triple.Left.Right;
                var assemblyName = triple.Right;

                foreach (var entry in attributedMethodsArr)
                {
                    if (entry.Diagnostic != null)
                        spc.ReportDiagnostic(entry.Diagnostic);
                }

                var interceptorsByNamespace = InterceptorDataProcessor.Process(attributedMethodsArr, callSitesArr);
                var generatedCode = SourceCodeGenerator.Generate(interceptorsByNamespace, assemblyName);

                spc.AddSource("ObservatorInterceptors.g.cs", SourceText.From(generatedCode, System.Text.Encoding.UTF8));
            });

            // Helper: recursively get all types in a namespace
            static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
            {
                foreach (var member in ns.GetTypeMembers())
                    yield return member;
                foreach (var nested in ns.GetNamespaceMembers())
                foreach (var t in GetAllTypes(nested))
                    yield return t;
            }
        }
    }
}
