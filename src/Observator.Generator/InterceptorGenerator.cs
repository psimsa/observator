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

namespace Observator.Generator;

[Generator]
public class InterceptorGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Entry point for the Observator Interceptor source generator.
    /// Registers syntax providers and output logic for tracing/interception.
    /// </summary>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var attributedMethods = RegisterAttributedMethodProvider(context);
        var interfaceMethods = RegisterInterfaceMethodProvider(context);
        var externalAttributedMethods = RegisterExternalAttributedMethodProvider(context);
        var allAttributedMethods = CombineAllAttributedMethods(attributedMethods, interfaceMethods, externalAttributedMethods);
        // Get call sites from all syntax trees in the current compilation and referenced source projects
        var allCallSites = context.CompilationProvider.SelectMany((compilation, ct) =>
        {
            var allSyntaxTrees = new List<SyntaxTree>(compilation.SyntaxTrees);

            foreach (var reference in compilation.References)
            {
                if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol asmSymbol)
                {
                    // If the reference is a CompilationReference, it means it's a source project reference
                    // and we can access its syntax trees.
                    if (reference is CompilationReference compRef)
                    {
                        allSyntaxTrees.AddRange(compRef.Compilation.SyntaxTrees);
                    }
                }
            }

            return allSyntaxTrees.SelectMany(tree =>
                tree.GetRoot(ct).DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>());
        })
        .Combine(context.CompilationProvider)
        .Select((tuple, ct) =>
        {
            var invocation = tuple.Item1;
            var compilation = tuple.Item2;
            var model = compilation.GetSemanticModel(invocation.SyntaxTree);
            return CallSiteAnalyzer.AnalyzeInvocationExpression(invocation, model, ct);
        })
        .Where(x => x != null)
        .Select((x, _) => x!);
        var assemblyInfo = RegisterAssemblyInfoProvider(context);
        var combined = CombineAllData(allAttributedMethods, allCallSites, assemblyInfo);


        RegisterSourceOutput(context, combined);
    }

    private static IncrementalValuesProvider<MethodToInterceptInfo> RegisterAttributedMethodProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: (ctx, ct) => MethodAnalyzer.AnalyzeMethodDeclaration(ctx.Node, ctx, ct))
            .Where(x => x != null)
            .Select((x, _) => x!); // Add null-forgiving operator as we've filtered out nulls
    }

    private static IncrementalValuesProvider<MethodToInterceptInfo> RegisterInterfaceMethodProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) =>
                    node is InterfaceDeclarationSyntax ids &&
                    (
                        ids.AttributeLists.SelectMany(al => al.Attributes)
                            .Any(attr =>
                                attr.Name.ToString().Contains("ObservatorTrace")
                            )
                        ||
                        ids.Members.OfType<MethodDeclarationSyntax>()
                            .SelectMany(m => m.AttributeLists.SelectMany(al => al.Attributes))
                            .Any(attr =>
                                attr.Name.ToString().Contains("ObservatorTrace")
                            )
                    ),
                transform: (ctx, ct) =>
                {
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol((InterfaceDeclarationSyntax)ctx.Node, ct) as INamedTypeSymbol;
                    if (symbol == null)
                    {
                        return Enumerable.Empty<MethodToInterceptInfo>();
                    }
                    return MethodAnalyzer.AnalyzeTypeDeclaration(symbol);
                }
            )
            .SelectMany((x, _) => x!); // Add null-forgiving operator as we've filtered out nulls
    }

    private static IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> RegisterExternalAttributedMethodProvider(IncrementalGeneratorInitializationContext context)
    {
        var relevantAssemblySymbols = context.CompilationProvider
            .SelectMany((compilation, _) =>
            {
                var relevantAssemblies = new List<IAssemblySymbol>();
                foreach (var reference in compilation.References)
                {
                    if (compilation.GetAssemblyOrModuleSymbol(reference) is IAssemblySymbol asmSymbol)
                    {
                        // For simplicity, consider all referenced assemblies as potentially relevant for now.
                        // A more precise filter can be added if needed, but the core issue seems to be
                        // identifying attributed methods and call sites within these assemblies.
                        relevantAssemblies.Add(asmSymbol);
                    }
                }
                return relevantAssemblies;
            });

        var attributedMethods = relevantAssemblySymbols
            .SelectMany((asmSymbol, _) => GetAllTypes(asmSymbol.GlobalNamespace))
            .SelectMany((typeSymbol, _) => typeSymbol.GetMembers().OfType<IMethodSymbol>())
            .Where(methodSymbol =>
            {
                return methodSymbol.GetAttributes().Any(attr =>
                {
                    var attrClass = attr.AttributeClass;
                    if (attrClass == null) return false;
                    return attrClass.ToDisplayString() == ObservatorConstants.ObservatorTraceAttributeFullName ||
                           attrClass.ToDisplayString() == ObservatorConstants.ObservatorGeneratedTestLibObservatorTraceAttributeFullName ||
                           attrClass.Name == ObservatorConstants.ObservatorTraceAttributeName ||
                           attrClass.Name == ObservatorConstants.ObservatorTraceShortName;
                });
            })
            .Select((methodSymbol, _) => new MethodToInterceptInfo(methodSymbol, null, false)); // Use constructor for external methods

        return attributedMethods.Collect(); // Collect all results into an ImmutableArray
    }

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetTypeMembers())
            yield return member;
        foreach (var nested in ns.GetNamespaceMembers())
        foreach (var t in GetAllTypes(nested))
            yield return t;
    }

    private static IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> CombineAllAttributedMethods(
        IncrementalValuesProvider<MethodToInterceptInfo> attributedMethods,
        IncrementalValuesProvider<MethodToInterceptInfo> interfaceMethods,
        IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> externalAttributedMethods)
    {
        var localAndInterfaceMethods = attributedMethods
            .Collect()
            .Combine(interfaceMethods.Collect())
            .Select((tuple, _) => tuple.Left.AddRange(tuple.Right));

        return localAndInterfaceMethods
            .Combine(externalAttributedMethods)
            .Select((tuple, _) => tuple.Left.AddRange(tuple.Right));
    }

    private static IncrementalValuesProvider<InvocationCallSiteInfo> RegisterCallSiteProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is InvocationExpressionSyntax,
                transform: (ctx, ct) => CallSiteAnalyzer.AnalyzeInvocationExpression(ctx.Node, ctx.SemanticModel, ct)) // Pass ctx.SemanticModel
            .Where(x => x != null)
            .Select((x, _) => x!); // Add null-forgiving operator as we've filtered out nulls
    }

    private static IncrementalValueProvider<string> RegisterAssemblyInfoProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider.Select((compilation, _) =>
        {
            var assemblyName = compilation.AssemblyName ?? "Unknown";
            return assemblyName;
        });
    }

    private static IncrementalValueProvider<(ImmutableArray<MethodToInterceptInfo>, ImmutableArray<InvocationCallSiteInfo>, string)> CombineAllData(
        IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> allAttributedMethods,
        IncrementalValuesProvider<InvocationCallSiteInfo> callSites, // Changed to non-nullable
        IncrementalValueProvider<string> assemblyInfo)
    {
        return allAttributedMethods.Combine(callSites.Collect()).Combine(assemblyInfo) // callSites.Collect() will now return ImmutableArray<InvocationCallSiteInfo>
            .Select((triple, _) => (triple.Left.Left, triple.Left.Right, triple.Right));
    }

    private static void RegisterSourceOutput(
        IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<(ImmutableArray<MethodToInterceptInfo>, ImmutableArray<InvocationCallSiteInfo>, string)> combined)
    {
        context.RegisterSourceOutput(combined, (spc, triple) =>
        {
            var attributedMethodsArr = triple.Item1;
            var callSitesArr = triple.Item2;
            var assemblyName = triple.Item3;

            foreach (var entry in attributedMethodsArr)
            {
                if (entry.Diagnostic != null)
                    spc.ReportDiagnostic(entry.Diagnostic);
            }

            var interceptorsByNamespace = InterceptorDataProcessor.Process(
                attributedMethodsArr,
                callSitesArr
            );
            var generatedCode = SourceCodeGenerator.Generate(interceptorsByNamespace);

            spc.AddSource("ObservatorInterceptors.g.cs", SourceText.From(generatedCode, System.Text.Encoding.UTF8));
        });
    }
}