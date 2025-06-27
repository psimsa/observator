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
        var callSites = RegisterCallSiteProvider(context);
        var assemblyInfo = RegisterAssemblyInfoProvider(context);
        var combined = CombineAllData(allAttributedMethods, callSites, assemblyInfo);

        RegisterSourceOutput(context, combined);
    }

    private static IncrementalValuesProvider<MethodToInterceptInfo?> RegisterAttributedMethodProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is MethodDeclarationSyntax,
                transform: (ctx, ct) => MethodAnalyzer.AnalyzeMethodDeclaration(ctx.Node, ctx, ct))
            .Where(x => x != null)
            .Select((x, _) => x);
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
                    MethodAnalyzer.AnalyzeTypeDeclaration(
                        ctx.SemanticModel.GetDeclaredSymbol((InterfaceDeclarationSyntax)ctx.Node, ct) as INamedTypeSymbol
                    )
            )
            .Where(x => x != null)
            .SelectMany((x, _) => x);
    }

    private static IncrementalValueProvider<List<MethodToInterceptInfo>> RegisterExternalAttributedMethodProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider.Select((compilation, _) =>
        {
            return GetAttributedMethodsFromReferences(compilation);
        });
    }

    private static List<MethodToInterceptInfo> GetAttributedMethodsFromReferences(Compilation compilation)
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

    private static IEnumerable<INamedTypeSymbol> GetAllTypes(INamespaceSymbol ns)
    {
        foreach (var member in ns.GetTypeMembers())
            yield return member;
        foreach (var nested in ns.GetNamespaceMembers())
        foreach (var t in GetAllTypes(nested))
            yield return t;
    }

    private static IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> CombineAllAttributedMethods(
        IncrementalValuesProvider<MethodToInterceptInfo?> attributedMethods,
        IncrementalValuesProvider<MethodToInterceptInfo> interfaceMethods,
        IncrementalValueProvider<List<MethodToInterceptInfo>> externalAttributedMethods)
    {
        return attributedMethods.Collect()
            .Combine(interfaceMethods.Collect())
            .Select((tuple, _) => tuple.Left.Concat(tuple.Right).ToImmutableArray())
            .Combine(externalAttributedMethods)
            .Select((tuple, _) =>
            {
                var localAndInterface = tuple.Left;
                var external = tuple.Right ?? new List<MethodToInterceptInfo>();
                return localAndInterface.AddRange(external);
            });
    }

    private static IncrementalValuesProvider<InvocationCallSiteInfo?> RegisterCallSiteProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: (node, _) => node is InvocationExpressionSyntax,
                transform: (ctx, ct) => CallSiteAnalyzer.AnalyzeInvocationExpression(ctx.Node, ctx, ct))
            .Where(x => x != null)
            .Select((x, _) => x);
    }

    private static IncrementalValueProvider<string> RegisterAssemblyInfoProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.CompilationProvider.Select((compilation, _) =>
        {
            var assemblyName = compilation.AssemblyName ?? "Unknown";
            return assemblyName;
        });
    }

    private static IncrementalValueProvider<(ImmutableArray<MethodToInterceptInfo>, ImmutableArray<InvocationCallSiteInfo?>, string)> CombineAllData(
        IncrementalValueProvider<ImmutableArray<MethodToInterceptInfo>> allAttributedMethods,
        IncrementalValuesProvider<InvocationCallSiteInfo?> callSites,
        IncrementalValueProvider<string> assemblyInfo)
    {
        return allAttributedMethods.Combine(callSites.Collect()).Combine(assemblyInfo)
            .Select((triple, _) => (triple.Left.Left, triple.Left.Right, triple.Right));
    }

    private static void RegisterSourceOutput(
        IncrementalGeneratorInitializationContext context,
        IncrementalValueProvider<(ImmutableArray<MethodToInterceptInfo>, ImmutableArray<InvocationCallSiteInfo?>, string)> combined)
    {
        context.RegisterSourceOutput(combined, (spc, triple) =>
        {
            var attributedMethodsArr = triple.Item1;
            var callSitesArr = triple.Item2;
            var assemblyName = triple.Item3;

            foreach (var entry in attributedMethodsArr)
            {
                if (entry?.Diagnostic != null)
                    spc.ReportDiagnostic(entry.Diagnostic);
            }

            var interceptorsByNamespace = InterceptorDataProcessor.Process(
                attributedMethodsArr.Where(x => x != null).ToImmutableArray(),
                callSitesArr.Where(x => x != null).ToImmutableArray()
            );
            var generatedCode = SourceCodeGenerator.Generate(interceptorsByNamespace);

            spc.AddSource("ObservatorInterceptors.g.cs", SourceText.From(generatedCode, System.Text.Encoding.UTF8));
        });
    }
}